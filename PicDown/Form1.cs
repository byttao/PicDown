using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Color = System.Drawing.Color;

// 确保你安装了 ImageSharp
// 引入 Jpeg 格式
using Image = SixLabors.ImageSharp.Image;
using HtmlDocument = HtmlAgilityPack.HtmlDocument; // 引入 Webp 格式

namespace PicDown
{
    public partial class Form1 : Form
    {
        private const string ImageSrcRegexPattern = @"<img\s+(?:[^>]*?)src=[""']?([^""'\s>]+)[""']?(?:[^>]*?)>";
        private string defaultProtocol = "https";
        private string downloadDirectory;

        private string fullHtmlContent;

        public Form1()
        {
            InitializeComponent();
            txtOutputFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void LogMessage(string message, LogLevel level)
        {
            // 在 UI 线程上更新 RichTextBox
            if (richTextBoxLog.InvokeRequired)
            {
                richTextBoxLog.Invoke(() => AppendLog(message, level));
            }
            else
            {
                AppendLog(message, level);
            }
        }

        private void AppendLog(string message, LogLevel level)
        {
            string prefix = "";
            Color color = Color.Black; // 默认颜色

            switch (level)
            {
                case LogLevel.Info:
                    prefix = "[INFO] ";
                    color = Color.Blue;
                    break;

                case LogLevel.Warning:
                    prefix = "[WARN] ";
                    color = Color.Orange;
                    break;

                case LogLevel.Error:
                    prefix = "[ERROR] ";
                    color = Color.Red;
                    break;

                case LogLevel.Success:
                    prefix = "[SUCCESS] ";
                    color = Color.Green;
                    break;

                case LogLevel.Processing:
                    prefix = "[PROCESS] ";
                    color = Color.Purple;
                    break;
            }

            // 格式化输出
            string formattedMessage = $"{DateTime.Now:HH:mm:ss} {prefix}{message}\r\n";

            // 记录颜色
            int startIndex = richTextBoxLog.Text.Length;
            richTextBoxLog.AppendText(formattedMessage);
            int endIndex = richTextBoxLog.Text.Length;

            richTextBoxLog.Select(startIndex, endIndex - startIndex);
            richTextBoxLog.SelectionColor = color;
            richTextBoxLog.SelectionCharOffset = 0; // 恢复默认偏移
            richTextBoxLog.DeselectAll(); // 取消选择

            // 自动滚动到底部
            richTextBoxLog.ScrollToCaret();
        }

        private void Convert(string webpFilePath, string jpgFilePath)
        {
            if (!File.Exists(webpFilePath))
            {
                LogMessage($"    -> WebP文件没有找到: {webpFilePath}", LogLevel.Error);
            }

            try
            {
                // 1. 使用 ImageSharp 加载 WebP 图片
                using (var image = Image.Load(webpFilePath))
                {
                    // 2. 保存为 JPG 格式
                    // SaveAsJpeg 方法需要指定 JpegEncoder，可以设置一些参数，例如质量
                    image.SaveAsJpeg(jpgFilePath, new JpegEncoder
                    {
                        Quality = 90
                    }); // Quality 范围 0-100
                }

                if (File.Exists(webpFilePath))
                {
                    File.Delete(webpFilePath);
                }

                LogMessage($"    -> 已成功转换保存: {webpFilePath} 到 {jpgFilePath}", LogLevel.Success);
            }
            catch (Exception ex)
            {
                LogMessage($"    [!] 处理错误 - {webpFilePath}: {ex.Message}", LogLevel.Error);
            }
        }

        private string GetIDFromUrl()
        {
            // 要使用的正则表达式
            string pattern = @"(?<=data-item\s*=\s*"")\d+(?="")";

            // 查找匹配项
            Match match = Regex.Match(fullHtmlContent, pattern);

            // 检查是否找到匹配项
            if (match.Success)
            {
                LogMessage($"    -> 当前链接ID为: {match.Value}", LogLevel.Success);
                // 找到了第一个匹配项，获取其值
                return match.Value;
            }

            return "UnknownID";
        }

        private List<(string Url, string Index)> ExtractVedioLinksFromDiv(string elementId)
        {
            var processedVedioLinks = new List<(string Url, string Index)>();
            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);
            // 3. 使用 XPath 定位 video 元素
            // XPath 语法：//tagName[@attributeName='attributeValue']
            // //video[@id='yourElementId']
            var videoNode = doc.DocumentNode.SelectSingleNode($"//video[@id='{elementId}']");

            // 4. 检查是否找到元素并获取 src 属性
            if (videoNode != null)
            {
                // 获取 src 属性的值
                var srcAttribute = videoNode.Attributes["src"];
                if (srcAttribute != null)
                {
                    string processedUrl = AddProtocolIfMissing(srcAttribute.Value);
                    processedUrl = processedUrl.Replace("&amp;", "&");
                    processedVedioLinks.Add((processedUrl, "1"));
                    return processedVedioLinks;
                }
            }

            LogMessage($"未找到 id 为 '{elementId}' 的 vedio 元素。未找到视频链接", LogLevel.Error);
            return processedVedioLinks;
        }

        public List<(string Url, string Titel)> ExtractSKUImageLinksFromDiv(string targetDivClassName)
        {
            var skuDataList = new List<(string Url, string Titel)>();

            int currentIndex = 1;
            List<string> suffixesToRemove = new List<string>
            {
                "_90x90q30.jpg_.webp",
                "_q50.jpg_.webp"
                // 可以根据需要添加更多可能的结尾
            };

            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);

            var targetDivs = doc.DocumentNode.SelectNodes($"//div[contains(@class, '{targetDivClassName}')]");

            if (targetDivs == null || !targetDivs.Any())
            {
                LogMessage($"未找到 class 为 '{targetDivClassName}' 的 div 元素。", LogLevel.Error);

                return skuDataList;
            }

            // 定位所有包含sku选项的div
            var skuItemDivs = targetDivs[0].SelectNodes("//div[contains(@class, 'skuItem--Z2AJB9Ew')]");

            if (skuItemDivs == null)
            {
                LogMessage($"未找到 class 为 '{"skuItem--Z2AJB9Ew"}' 的 div 元素。", LogLevel.Error);
                return skuDataList; // 没有找到sku项
            }

            foreach (var skuItemDiv in skuItemDivs)
            {
                // 提取"颜色分类"（或者其他属性名）的span的title
                var attributeNameNode = skuItemDiv.SelectSingleNode(".//div[contains(@class, 'ItemLabel--psS1SOyC')]/span");
                string attributeName = attributeNameNode?.Attributes["title"]?.Value;

                // 定位所有sku的valueItem div
                var valueItemDivs = skuItemDiv.SelectNodes(".//div[contains(@class, 'valueItem--smR4pNt4')]");

                if (valueItemDivs != null)
                {
                    foreach (var valueItemDiv in valueItemDivs)
                    {
                        string ImageUrl = "";
                        string Title = "";

                        // 提取图片链接
                        var imgNode = valueItemDiv.SelectSingleNode(".//img");
                        if (imgNode != null && imgNode.Attributes["src"] != null)
                        {
                            ImageUrl = imgNode.Attributes["src"].Value;
                        }

                        // 提取span的title (SKU值)
                        var spanNode = valueItemDiv.SelectSingleNode(".//span");
                        if (spanNode != null && spanNode.Attributes["title"] != null)
                        {
                            Title = spanNode.Attributes["title"].Value;
                        }

                        if (!string.IsNullOrEmpty(ImageUrl))
                        {
                            // 使用 LINQ 查找所有后缀的 LastIndexOf，并选择最大的那个索引
                            int bestMatchIndex = suffixesToRemove
                                .Select(suffix => ImageUrl.LastIndexOf(suffix)) // 获取每个后缀的 LastIndexOf 结果
                                .Where(index => index != -1) // 过滤掉未找到的 (-1)
                                .DefaultIfEmpty(-1) // 如果没有找到任何后缀，则默认为 -1
                                .Max(); // 找到最大的索引

                            ImageUrl = bestMatchIndex != -1
                                ? ImageUrl.Substring(0, bestMatchIndex)
                                : ImageUrl; // 如果 bestMatchIndex 是 -1，说明没有匹配到，url 不变

                            skuDataList.Add((ImageUrl, Title));
                        }
                    }
                }
            }

            return skuDataList;
        }

        public List<(string Url, string Index)> ExtractImageLinksFromDiv(string targetDivClassName)
        {
            var processedImageLinks = new List<(string Url, string Index)>();
            int currentIndex = 1;
            List<string> suffixesToRemove = new List<string>
            {
                "_90x90q30.jpg_.webp",
                "_q50.jpg_.webp"
                // 可以根据需要添加更多可能的结尾
            };

            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);

            var targetDivs = doc.DocumentNode.SelectNodes($"//div[contains(@class, '{targetDivClassName}')]");

            if (targetDivs == null || !targetDivs.Any())
            {
                LogMessage($"未找到 class 为 '{targetDivClassName}' 的 div 元素。", LogLevel.Error);

                return processedImageLinks;
            }

            Regex regex = new Regex(ImageSrcRegexPattern, RegexOptions.IgnoreCase);

            foreach (var divNode in targetDivs)
            {
                string divHtml = divNode.InnerHtml;
                MatchCollection matches = regex.Matches(divHtml);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string src = match.Groups[1].Value;
                        string processedUrl = AddProtocolIfMissing(src);

                        if (!string.IsNullOrEmpty(processedUrl))
                        {
                            // 使用 LINQ 查找所有后缀的 LastIndexOf，并选择最大的那个索引
                            int bestMatchIndex = suffixesToRemove
                                .Select(suffix => processedUrl.LastIndexOf(suffix)) // 获取每个后缀的 LastIndexOf 结果
                                .Where(index => index != -1) // 过滤掉未找到的 (-1)
                                .DefaultIfEmpty(-1) // 如果没有找到任何后缀，则默认为 -1
                                .Max(); // 找到最大的索引

                            processedUrl = bestMatchIndex != -1
                                ? processedUrl.Substring(0, bestMatchIndex)
                                : processedUrl; // 如果 bestMatchIndex 是 -1，说明没有匹配到，url 不变

                            processedImageLinks.Add((processedUrl, (currentIndex++).ToString("D2")));
                        }
                    }
                }
            }

            return processedImageLinks;
        }

        private string AddProtocolIfMissing(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.StartsWith("//"))
            {
                return $"{defaultProtocol}:{url}";
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            LogMessage($"    发现相对路径 URL '{url}'，但未提供 baseUrl，无法完整处理。", LogLevel.Warning);
            return null;
        }

        public async Task DownloadVedioParallelAsync(List<(string Url, string Index)> InfoList)
        {
            if (InfoList.Any())
            {
                //string categoryPath = Path.Combine(downloadDirectory, "视频");
                //if (!Directory.Exists(categoryPath))
                //{
                //    Directory.CreateDirectory(categoryPath);
                //}
                string category = "视频";

                LogMessage($"准备下载 视频 共计 {InfoList.Count} 个视频...", LogLevel.Processing);
                var task = InfoList.Select(info => DownloadImageAsync(info.Url, info.Index, category,1, true));
                await Task.WhenAll(task);
                LogMessage("所有视频下载完成", LogLevel.Success);
            }
            else
            {
                LogMessage("没有找到有效的视频链接进行下载", LogLevel.Error);
            }
        }

        public async Task DownloadImagesParallelAsync(List<(string Url, string Index)> imageInfoList, string category)
        {
            if (imageInfoList.Any())
            {
                //string categoryPath = Path.Combine(downloadDirectory, category);
                //if (!Directory.Exists(categoryPath))
                //{
                //    Directory.CreateDirectory(categoryPath);
                //}

                LogMessage($"准备下载 {category} 共计 {imageInfoList.Count} 张图片...", LogLevel.Processing);

                var task = imageInfoList.Select((imageInfo,I) => DownloadImageAsync(imageInfo.Url, imageInfo.Index, category,I, false));
                await Task.WhenAll(task);

                LogMessage("所有图片下载完成", LogLevel.Success);
                /*
                if (category == "主图")
                {
                    LogMessage("开始转换主图格式", LogLevel.Processing);
                    var webpFiles = Directory.GetFiles(categoryPath, "*.webp");
                    foreach (var webpFile in webpFiles)
                    {
                        string jpgFilePath = Path.ChangeExtension(webpFile, ".jpg");
                        Convert(webpFile, jpgFilePath);
                    }
                }
                */
            }
            else
            {
                LogMessage("没有找到有效的图片链接进行下载", LogLevel.Error);
            }
        }

        private async Task DownloadImageAsync(string imageUrl, string title, string category,int index, bool isvedio)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(imageUrl);
                    response.EnsureSuccessStatusCode();

                    string extension = Path.GetExtension(imageUrl);
                    if (string.IsNullOrEmpty(extension))
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (!string.IsNullOrEmpty(contentType))
                        {
                            switch (contentType.ToLower())
                            {
                                case "image/jpeg":
                                    extension = ".jpg";
                                    break;

                                case "image/png":
                                    extension = ".png";
                                    break;

                                case "image/gif":
                                    extension = ".gif";
                                    break;

                                case "image/bmp":
                                    extension = ".bmp";
                                    break;

                                case "image/webp":
                                    extension = ".webp";
                                    break;

                                case "video/mp4":
                                    extension = ".mp4";
                                    break; // 增加对 MP4 的支持

                                default:
                                    extension = ".jpg";
                                    break;
                            }
                        }
                        else
                        {
                            extension = ".jpg";
                        }
                    }

                    if (isvedio)
                    {
                        extension = ".mp4";
                    }
                    string fileName;
                    if (category == "SKU图")
                    {
                        fileName = $"{category}_{index:D2}_{title}{extension}";
                    }
                    else
                    {
                        fileName = $"{category}_{title}{extension}";
                    }

                    var filePath = Path.Combine(downloadDirectory, fileName);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(filePath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                    LogMessage($"已下载: {fileName} (来自: {imageUrl})", LogLevel.Success);
                }
                catch (HttpRequestException ex)
                {
                    LogMessage($"下载失败: {imageUrl} - {ex.Message}", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    LogMessage($"处理链接 {imageUrl} 时发生错误: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private async void btnDown_Click(object sender, EventArgs e)
        {
            if (rtbHtml.Text == "")
            {
                LogMessage("窗口未粘贴源代码，请重试！", LogLevel.Error);
                return;
            }

            btnDown.Enabled = false;
            fullHtmlContent = rtbHtml.Text;

            string downloadFolder = "TargetedDownloadsFromFile";
            string protocol = "https"; // 默认协议
            downloadDirectory = Path.Combine(txtOutputFolder.Text, GetIDFromUrl());
            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            // 2. 提取 thumbnails--v976to2t 中的图片链接
            LogMessage("正在提取主图清单", LogLevel.Processing);
            List<(string Url, string Index)> thumbnailImages = ExtractImageLinksFromDiv("thumbnails--v976to2t");

            // 3. 提取 descV8-container 中的图片链接
            LogMessage("正在提取详情图清单", LogLevel.Processing);
            List<(string Url, string Index)> descImages = ExtractImageLinksFromDiv("descV8-container");

            // 4. 提取 content--DIGuLqdf 中的图片链接
            LogMessage("正在提取SKU图清单", LogLevel.Processing);
            List<(string Url, string Index)> SKUImages = ExtractSKUImageLinksFromDiv("content--DIGuLqdf");

            // 4. 提取 videox-video-el 中的视频链接
            LogMessage("正在提取视频链接", LogLevel.Processing);
            List<(string Url, string Index)> Vedios = ExtractVedioLinksFromDiv("videox-video-el");

            // 5. 下载所有图片
            await DownloadImagesParallelAsync(thumbnailImages, "主图");
            await DownloadImagesParallelAsync(SKUImages, "SKU图");
            await DownloadImagesParallelAsync(descImages, "详情图");
            await DownloadVedioParallelAsync(Vedios);
            LogMessage("所有操作已完成！", LogLevel.Success);
            rtbHtml.Text = "";
            btnDown.Enabled = true;
        }

        private void btnFloder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                // 设置对话框的初始路径
                if (!string.IsNullOrEmpty(txtOutputFolder.Text) && Directory.Exists(txtOutputFolder.Text))
                {
                    fbd.SelectedPath = txtOutputFolder.Text;
                }
                else
                {
                    // 否则，使用我的文档作为默认
                    fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }

                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void txtOutputFolder_TextChanged(object sender, EventArgs e)
        {
            LogMessage($"图包输出文件夹已设置为: \"{txtOutputFolder.Text}\"", LogLevel.Info);
        }

        // 日志输出函数
        private enum LogLevel
        {
            Info,
            Warning,
            Error,
            Success,
            Processing
        }
    }
}
