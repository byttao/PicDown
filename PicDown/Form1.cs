using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Color = System.Drawing.Color;

// ȷ���㰲װ�� ImageSharp
// ���� Jpeg ��ʽ
using Image = SixLabors.ImageSharp.Image;
using HtmlDocument = HtmlAgilityPack.HtmlDocument; // ���� Webp ��ʽ

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
            // �� UI �߳��ϸ��� RichTextBox
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
            Color color = Color.Black; // Ĭ����ɫ

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

            // ��ʽ�����
            string formattedMessage = $"{DateTime.Now:HH:mm:ss} {prefix}{message}\r\n";

            // ��¼��ɫ
            int startIndex = richTextBoxLog.Text.Length;
            richTextBoxLog.AppendText(formattedMessage);
            int endIndex = richTextBoxLog.Text.Length;

            richTextBoxLog.Select(startIndex, endIndex - startIndex);
            richTextBoxLog.SelectionColor = color;
            richTextBoxLog.SelectionCharOffset = 0; // �ָ�Ĭ��ƫ��
            richTextBoxLog.DeselectAll(); // ȡ��ѡ��

            // �Զ��������ײ�
            richTextBoxLog.ScrollToCaret();
        }

        private void Convert(string webpFilePath, string jpgFilePath)
        {
            if (!File.Exists(webpFilePath))
            {
                LogMessage($"    -> WebP�ļ�û���ҵ�: {webpFilePath}", LogLevel.Error);
            }

            try
            {
                // 1. ʹ�� ImageSharp ���� WebP ͼƬ
                using (var image = Image.Load(webpFilePath))
                {
                    // 2. ����Ϊ JPG ��ʽ
                    // SaveAsJpeg ������Ҫָ�� JpegEncoder����������һЩ��������������
                    image.SaveAsJpeg(jpgFilePath, new JpegEncoder
                    {
                        Quality = 90
                    }); // Quality ��Χ 0-100
                }

                if (File.Exists(webpFilePath))
                {
                    File.Delete(webpFilePath);
                }

                LogMessage($"    -> �ѳɹ�ת������: {webpFilePath} �� {jpgFilePath}", LogLevel.Success);
            }
            catch (Exception ex)
            {
                LogMessage($"    [!] ������� - {webpFilePath}: {ex.Message}", LogLevel.Error);
            }
        }

        private string GetIDFromUrl()
        {
            // Ҫʹ�õ�������ʽ
            string pattern = @"(?<=data-item\s*=\s*"")\d+(?="")";

            // ����ƥ����
            Match match = Regex.Match(fullHtmlContent, pattern);

            // ����Ƿ��ҵ�ƥ����
            if (match.Success)
            {
                LogMessage($"    -> ��ǰ����IDΪ: {match.Value}", LogLevel.Success);
                // �ҵ��˵�һ��ƥ�����ȡ��ֵ
                return match.Value;
            }

            return "UnknownID";
        }

        private List<(string Url, string Index)> ExtractVedioLinksFromDiv(string elementId)
        {
            var processedVedioLinks = new List<(string Url, string Index)>();
            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);
            // 3. ʹ�� XPath ��λ video Ԫ��
            // XPath �﷨��//tagName[@attributeName='attributeValue']
            // //video[@id='yourElementId']
            var videoNode = doc.DocumentNode.SelectSingleNode($"//video[@id='{elementId}']");

            // 4. ����Ƿ��ҵ�Ԫ�ز���ȡ src ����
            if (videoNode != null)
            {
                // ��ȡ src ���Ե�ֵ
                var srcAttribute = videoNode.Attributes["src"];
                if (srcAttribute != null)
                {
                    string processedUrl = AddProtocolIfMissing(srcAttribute.Value);
                    processedUrl = processedUrl.Replace("&amp;", "&");
                    processedVedioLinks.Add((processedUrl, "1"));
                    return processedVedioLinks;
                }
            }

            LogMessage($"δ�ҵ� id Ϊ '{elementId}' �� vedio Ԫ�ء�δ�ҵ���Ƶ����", LogLevel.Error);
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
                // ���Ը�����Ҫ��Ӹ�����ܵĽ�β
            };

            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);

            var targetDivs = doc.DocumentNode.SelectNodes($"//div[contains(@class, '{targetDivClassName}')]");

            if (targetDivs == null || !targetDivs.Any())
            {
                LogMessage($"δ�ҵ� class Ϊ '{targetDivClassName}' �� div Ԫ�ء�", LogLevel.Error);

                return skuDataList;
            }

            // ��λ���а���skuѡ���div
            var skuItemDivs = targetDivs[0].SelectNodes("//div[contains(@class, 'skuItem--Z2AJB9Ew')]");

            if (skuItemDivs == null)
            {
                LogMessage($"δ�ҵ� class Ϊ '{"skuItem--Z2AJB9Ew"}' �� div Ԫ�ء�", LogLevel.Error);
                return skuDataList; // û���ҵ�sku��
            }

            foreach (var skuItemDiv in skuItemDivs)
            {
                // ��ȡ"��ɫ����"��������������������span��title
                var attributeNameNode = skuItemDiv.SelectSingleNode(".//div[contains(@class, 'ItemLabel--psS1SOyC')]/span");
                string attributeName = attributeNameNode?.Attributes["title"]?.Value;

                // ��λ����sku��valueItem div
                var valueItemDivs = skuItemDiv.SelectNodes(".//div[contains(@class, 'valueItem--smR4pNt4')]");

                if (valueItemDivs != null)
                {
                    foreach (var valueItemDiv in valueItemDivs)
                    {
                        string ImageUrl = "";
                        string Title = "";

                        // ��ȡͼƬ����
                        var imgNode = valueItemDiv.SelectSingleNode(".//img");
                        if (imgNode != null && imgNode.Attributes["src"] != null)
                        {
                            ImageUrl = imgNode.Attributes["src"].Value;
                        }

                        // ��ȡspan��title (SKUֵ)
                        var spanNode = valueItemDiv.SelectSingleNode(".//span");
                        if (spanNode != null && spanNode.Attributes["title"] != null)
                        {
                            Title = spanNode.Attributes["title"].Value;
                        }

                        if (!string.IsNullOrEmpty(ImageUrl))
                        {
                            // ʹ�� LINQ �������к�׺�� LastIndexOf����ѡ�������Ǹ�����
                            int bestMatchIndex = suffixesToRemove
                                .Select(suffix => ImageUrl.LastIndexOf(suffix)) // ��ȡÿ����׺�� LastIndexOf ���
                                .Where(index => index != -1) // ���˵�δ�ҵ��� (-1)
                                .DefaultIfEmpty(-1) // ���û���ҵ��κκ�׺����Ĭ��Ϊ -1
                                .Max(); // �ҵ���������

                            ImageUrl = bestMatchIndex != -1
                                ? ImageUrl.Substring(0, bestMatchIndex)
                                : ImageUrl; // ��� bestMatchIndex �� -1��˵��û��ƥ�䵽��url ����

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
                // ���Ը�����Ҫ��Ӹ�����ܵĽ�β
            };

            var doc = new HtmlDocument();
            doc.LoadHtml(fullHtmlContent);

            var targetDivs = doc.DocumentNode.SelectNodes($"//div[contains(@class, '{targetDivClassName}')]");

            if (targetDivs == null || !targetDivs.Any())
            {
                LogMessage($"δ�ҵ� class Ϊ '{targetDivClassName}' �� div Ԫ�ء�", LogLevel.Error);

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
                            // ʹ�� LINQ �������к�׺�� LastIndexOf����ѡ�������Ǹ�����
                            int bestMatchIndex = suffixesToRemove
                                .Select(suffix => processedUrl.LastIndexOf(suffix)) // ��ȡÿ����׺�� LastIndexOf ���
                                .Where(index => index != -1) // ���˵�δ�ҵ��� (-1)
                                .DefaultIfEmpty(-1) // ���û���ҵ��κκ�׺����Ĭ��Ϊ -1
                                .Max(); // �ҵ���������

                            processedUrl = bestMatchIndex != -1
                                ? processedUrl.Substring(0, bestMatchIndex)
                                : processedUrl; // ��� bestMatchIndex �� -1��˵��û��ƥ�䵽��url ����

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

            LogMessage($"    �������·�� URL '{url}'����δ�ṩ baseUrl���޷���������", LogLevel.Warning);
            return null;
        }

        public async Task DownloadVedioParallelAsync(List<(string Url, string Index)> InfoList)
        {
            if (InfoList.Any())
            {
                //string categoryPath = Path.Combine(downloadDirectory, "��Ƶ");
                //if (!Directory.Exists(categoryPath))
                //{
                //    Directory.CreateDirectory(categoryPath);
                //}
                string category = "��Ƶ";

                LogMessage($"׼������ ��Ƶ ���� {InfoList.Count} ����Ƶ...", LogLevel.Processing);
                var task = InfoList.Select(info => DownloadImageAsync(info.Url, info.Index, category,1, true));
                await Task.WhenAll(task);
                LogMessage("������Ƶ�������", LogLevel.Success);
            }
            else
            {
                LogMessage("û���ҵ���Ч����Ƶ���ӽ�������", LogLevel.Error);
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

                LogMessage($"׼������ {category} ���� {imageInfoList.Count} ��ͼƬ...", LogLevel.Processing);

                var task = imageInfoList.Select((imageInfo,I) => DownloadImageAsync(imageInfo.Url, imageInfo.Index, category,I, false));
                await Task.WhenAll(task);

                LogMessage("����ͼƬ�������", LogLevel.Success);
                /*
                if (category == "��ͼ")
                {
                    LogMessage("��ʼת����ͼ��ʽ", LogLevel.Processing);
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
                LogMessage("û���ҵ���Ч��ͼƬ���ӽ�������", LogLevel.Error);
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
                                    break; // ���Ӷ� MP4 ��֧��

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
                    if (category == "SKUͼ")
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

                    LogMessage($"������: {fileName} (����: {imageUrl})", LogLevel.Success);
                }
                catch (HttpRequestException ex)
                {
                    LogMessage($"����ʧ��: {imageUrl} - {ex.Message}", LogLevel.Error);
                }
                catch (Exception ex)
                {
                    LogMessage($"�������� {imageUrl} ʱ��������: {ex.Message}", LogLevel.Error);
                }
            }
        }

        private async void btnDown_Click(object sender, EventArgs e)
        {
            if (rtbHtml.Text == "")
            {
                LogMessage("����δճ��Դ���룬�����ԣ�", LogLevel.Error);
                return;
            }

            btnDown.Enabled = false;
            fullHtmlContent = rtbHtml.Text;

            string downloadFolder = "TargetedDownloadsFromFile";
            string protocol = "https"; // Ĭ��Э��
            downloadDirectory = Path.Combine(txtOutputFolder.Text, GetIDFromUrl());
            if (!Directory.Exists(downloadDirectory))
            {
                Directory.CreateDirectory(downloadDirectory);
            }

            // 2. ��ȡ thumbnails--v976to2t �е�ͼƬ����
            LogMessage("������ȡ��ͼ�嵥", LogLevel.Processing);
            List<(string Url, string Index)> thumbnailImages = ExtractImageLinksFromDiv("thumbnails--v976to2t");

            // 3. ��ȡ descV8-container �е�ͼƬ����
            LogMessage("������ȡ����ͼ�嵥", LogLevel.Processing);
            List<(string Url, string Index)> descImages = ExtractImageLinksFromDiv("descV8-container");

            // 4. ��ȡ content--DIGuLqdf �е�ͼƬ����
            LogMessage("������ȡSKUͼ�嵥", LogLevel.Processing);
            List<(string Url, string Index)> SKUImages = ExtractSKUImageLinksFromDiv("content--DIGuLqdf");

            // 4. ��ȡ videox-video-el �е���Ƶ����
            LogMessage("������ȡ��Ƶ����", LogLevel.Processing);
            List<(string Url, string Index)> Vedios = ExtractVedioLinksFromDiv("videox-video-el");

            // 5. ��������ͼƬ
            await DownloadImagesParallelAsync(thumbnailImages, "��ͼ");
            await DownloadImagesParallelAsync(SKUImages, "SKUͼ");
            await DownloadImagesParallelAsync(descImages, "����ͼ");
            await DownloadVedioParallelAsync(Vedios);
            LogMessage("���в�������ɣ�", LogLevel.Success);
            rtbHtml.Text = "";
            btnDown.Enabled = true;
        }

        private void btnFloder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                // ���öԻ���ĳ�ʼ·��
                if (!string.IsNullOrEmpty(txtOutputFolder.Text) && Directory.Exists(txtOutputFolder.Text))
                {
                    fbd.SelectedPath = txtOutputFolder.Text;
                }
                else
                {
                    // ����ʹ���ҵ��ĵ���ΪĬ��
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
            LogMessage($"ͼ������ļ���������Ϊ: \"{txtOutputFolder.Text}\"", LogLevel.Info);
        }

        // ��־�������
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
