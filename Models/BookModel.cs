//using ABI.System;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.IO.Hashing;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using VersOne.Epub;
using Windows.Graphics.Printing3D;
//using Uri = System.Uri;

using CommunityToolkit.Mvvm.ComponentModel;
using NotEdgeForEpubWpf.Utils;
using System.Text.Json.Serialization;

namespace NotEdgeForEpubWpf.Models
{

    public class BookModel
    {

        //public readonly static string storePathBase = Path.Combine(/*Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)*/"C:\\D\\CODE\\EPUBreader\\DevelopCache", "NEFEWdata");
        //public readonly static string cachePathBase = Path.Combine(/*Path.GetTempPath() */"C:\\D\\CODE\\EPUBreader\\DevelopCache", "NEFEWcache");
        public readonly static string storePathBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NEFEWstore");
        public readonly static string cachePathBase = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NEFEWcache");
        //public readonly EpubBookRef bookRef;
        public readonly string bookHash, cachePathBook, storePathBook, cachePathExtracted, cachePathPreprocessed,cachePathTemp;
        
        

        public readonly List<ReadingOrderPair> readingOrderPairs;
        public readonly ObservableCollection<NavigationNestItem> navigationItems;
        public readonly Dictionary<string, string> htmlPathDict;
        
        
        public static async Task<BookModel> BookModelFactoryFromZipAsync(EpubBookRef bookRef,Stream zipStream)
        {
            string bookHash="";
            using (var memStream = new MemoryStream())
            {
                await zipStream.CopyToAsync(memStream).ConfigureAwait(false);
                zipStream.Position = memStream.Position = 0;
                bookHash=await MyUtils.HashFromStreamAsync(memStream).ConfigureAwait(false);
            }
            string cachePathBook = Path.Combine(cachePathBase, bookHash);
            string storePathBook = Path.Combine(storePathBase, bookHash);
            string cachePathExtracted = Path.Combine(cachePathBook, "Extracted");
            string cachePathPreprocessed = Path.Combine(cachePathBook, "Preprocessed");
            if (!Directory.Exists(cachePathExtracted))
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(zipStream, cachePathExtracted)).ConfigureAwait(false);
            }
            if (!Directory.Exists(cachePathPreprocessed))
            {
                await PreprocessAndCacheAsync(bookRef, bookHash, cachePathBook).ConfigureAwait(false);
            }
            List<ReadingOrderPair>? readingOrderParis;
            ObservableCollection<NavigationNestItem>? navigationItems;
            Dictionary<string, string>? htmlPathDict;
            try
            {
                readingOrderParis
                    = JsonSerializer.Deserialize<List<ReadingOrderPair>>(
                        File.ReadAllText(
                            Path.Combine(cachePathPreprocessed, PreprocessedReadingOrderListName)
                        )
                    );
            }
            catch(Exception ex)
            {
                throw new JsonException("fail to load or deserialize reading-order path pairs", ex);
            }
            if (readingOrderParis == null)
            {
                throw new JsonException("fail to load or deserialize reading-order path pairs");
            }
            try
            {
                navigationItems
                    = JsonSerializer.Deserialize<ObservableCollection<NavigationNestItem>>(
                        File.ReadAllText(
                            Path.Combine(cachePathPreprocessed, PreprocessedNavigationItemListName)
                        ),
                        new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve }
                    );
            }
            catch(Exception ex)
            {
                throw new JsonException("fail to load or deserialize navigation list", ex);
            }
            if(navigationItems == null)
            {
                throw new JsonException("fail to load or deserialize navigation list");
            }
            try
            {
                htmlPathDict
                    = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        File.ReadAllText(
                            Path.Combine(cachePathPreprocessed, PreprocessedHtmlPathDictName)
                        )
                    );
            }
            catch (Exception ex)
            {
                throw new JsonException("fail to load or deserialize html path dictionary", ex);
            }
            if (htmlPathDict == null)
            {
                throw new JsonException("fail to load or deserialize html path dictionary");
            }
            return new(bookHash, cachePathBook, storePathBook,readingOrderParis, navigationItems, htmlPathDict);

        }
        private BookModel(string bookHash, string cachePathBook, string storePathBook,List<ReadingOrderPair>readingOrderPairs, ObservableCollection<NavigationNestItem>navigationItems,Dictionary<string,string>htmlPathDict)
        {
            this.bookHash = bookHash;
            this.cachePathBook = cachePathBook;
            this.storePathBook = storePathBook;
            this.readingOrderPairs = readingOrderPairs;
            this.navigationItems = navigationItems;
            this.htmlPathDict = htmlPathDict;
            this.cachePathExtracted = Path.Combine(cachePathBook, "Extracted");
            this.cachePathPreprocessed = Path.Combine(cachePathBook, "Preprocessed");
        }
        private static async Task<(HtmlDocument,Dictionary<string,string>)> HtmlPreprocessAndCacheAsync(EpubLocalTextContentFile html, string cachePathBook)
        {
            string cachePathExtracted = Path.Combine(cachePathBook, "Extracted");
            string cachePathDownloaded = Path.Combine(cachePathBook, "Downloaded");
            Directory.CreateDirectory(cachePathDownloaded);
            Dictionary<string, string> resourceCachePath = [];
            var htmlDoc = new HtmlDocument(); htmlDoc.LoadHtml(html.Content);
            using HttpClient httpClient = new HttpClient();
            static bool SkipUrl(string? url)
            {
                if (url == null||url=="") return true;
                if (url.StartsWith("//")) return false;
                if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult))
                {
                    // not skip only http and https schemes for a traditional web address.
                    return !uriResult.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                           !uriResult.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
                }
                if (Uri.TryCreate(url, UriKind.Relative, out uriResult)) return false;
                return true;
            }
            var DealUrl = async (HtmlNode node, string urlSrc) =>
            {
                string originalUrl = node.GetAttributeValue(urlSrc, "");
                if (originalUrl == "")
                {
                    node.Attributes.Remove(urlSrc);
                    return;
                }
                if (SkipUrl(originalUrl)) return;
                if (originalUrl.StartsWith("http:", StringComparison.OrdinalIgnoreCase) ||
                   originalUrl.StartsWith("https:", StringComparison.OrdinalIgnoreCase))
                {
                    if (node.Name.Equals("a", StringComparison.OrdinalIgnoreCase))
                        return;
                    try
                    {
                        Uri url = new Uri(originalUrl);
                        string extension = Path.GetExtension(url.AbsolutePath);
                        HttpResponseMessage? response = null;
                        if (string.IsNullOrEmpty(extension))
                        {
                            response = await httpClient.GetAsync(originalUrl);
                            response.EnsureSuccessStatusCode();
                            string? mediaType = response.Content?.Headers?.ContentType?.MediaType;
                            if ((mediaType != null && MyUtils.MimeMapping.TryGetValue(mediaType, out string? mappedExtension)))
                            {
                                extension = mappedExtension;
                            }
                            else
                            {
                                extension = ".bin"; // Default fallback extension.
                            }
                        }
                        string hashUrl = MyUtils.EncodeBase64FromString(originalUrl);
                        string targetPath = Path.Combine(cachePathDownloaded, hashUrl + extension);
                        if (!File.Exists(targetPath))
                        {
                            if (response == null) response = await httpClient.GetAsync(originalUrl);
                            if (response.Content == null)
                            {
                                throw new HttpRequestException("Response of remote source does not contain any content.");
                            }
                            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
                            await using FileStream fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
                            await contentStream.CopyToAsync(fileStream);
                            node.SetAttributeValue(urlSrc, "file:///"+targetPath);
                            resourceCachePath[originalUrl]=targetPath;
                        }
                        else resourceCachePath[originalUrl]=targetPath;
                    }
                    catch (HttpRequestException httpEx)
                    {

                    }
                }
                else
                {
                    string fullPath = MyUtils.PathInFileResovle(html.FilePath, originalUrl);
                    fullPath = Path.GetFullPath(Path.Combine(cachePathExtracted,fullPath));
                    if (File.Exists(fullPath))
                    {
                        node.SetAttributeValue(urlSrc, "file:///" + fullPath);
                        resourceCachePath[originalUrl]=fullPath;
                    }
                    else
                    {
                        node.Attributes.Remove(urlSrc);
                    }
                }
            };

            var hrefNodes = htmlDoc.DocumentNode.SelectNodes("//*[@href]");
            if (hrefNodes != null)
            {
                foreach (var node in hrefNodes)
                {
                    await DealUrl(node, "href");
                }
            }
            var srcNodes = htmlDoc.DocumentNode.SelectNodes("//*[@src]");
            if (srcNodes != null)
            {
                foreach (var node in srcNodes)
                {
                    await DealUrl(node, "src");
                }
            }
            //var xlinkNodes = htmlDoc.DocumentNode.SelectNodes("//*[@*[local-name()='href' and namespace-uri()='http://www.w3.org/1999/xlink']]");
            var xlinkNodes = htmlDoc.DocumentNode.Descendants()
               .Where(node => node.Attributes != null &&
                              node.Attributes.Any(attr =>
                                  attr.Name.Equals("xlink:href", StringComparison.OrdinalIgnoreCase)))
               .ToList();
            if (xlinkNodes != null)
            {
                foreach (var node in xlinkNodes)
                {
                    await DealUrl(node, "xlink:href");
                }
            }
            return (htmlDoc, resourceCachePath);
        }
        private const string PreprocessedReadingOrderListName= "ReadingOrder_CurrentPath_OriginPath_Pair.json";
        private const string PreprocessedResourceCacheDictName = "Resource_Cache_Path_Dictionary.json";
        private const string PreprocessedNavigationItemListName = "NavigationNestItemList.json";
        private const string PreprocessedHtmlPathDictName = "Html_PathDict.json";
        private static async Task PreprocessAndCacheAsync(EpubBookRef bookRef, string bookHash, string cachePathBook)
        {

            string cachePathExtracted = Path.Combine(cachePathBook, "Extracted");
            string cachePathPreprocessed = Path.Combine(cachePathBook, "Preprocessed");
            Directory.CreateDirectory(cachePathPreprocessed);
            try
            {
                ReadOnlyCollection<EpubLocalTextContentFile> localHtml = await ReadLocalTextContentFilesAsync(bookRef.Content.Html.Local);
                //var localHtmlByKey = localHtml.ToDictionary(html => html.Key);
                var localHtmlByPath = localHtml.ToDictionary(html => html.FilePath);
                List<EpubLocalTextContentFileRef> readingOrderRefs = await bookRef.GetReadingOrderAsync().ConfigureAwait(false);
                List<EpubLocalTextContentFile> readingOrder = readingOrderRefs.Select(ARef => localHtmlByPath[ARef.FilePath]).ToList();
                List<EpubNavigationItemRef>? navigationRefs =await bookRef.GetNavigationAsync().ConfigureAwait(false);
                Dictionary<string, string> resourceCachePath = [];
                List<ReadingOrderPair> readingOrderPairs = [];
                int index = 0;
                foreach (var html in readingOrder)
                {
                    var (doc, dic) = await HtmlPreprocessAndCacheAsync(html, cachePathBook);
                    resourceCachePath = resourceCachePath.Union(dic).ToDictionary();
                    string docPath = Path.Combine(cachePathPreprocessed, $"{index++}.html");
                    using (var fileStream = new FileStream(docPath, FileMode.Create, FileAccess.Write))
                    {
                        doc.Save(fileStream);
                    }
                    readingOrderPairs.Add(
                            new ReadingOrderPair
                            {
                                PathAfterProcess = docPath,
                                PathOriginalInEpub = html.FilePath
                            }
                        );
                }
                ObservableCollection<NavigationNestItem> navigationItems = [];
                Dictionary<string,string> htmlPathDict = readingOrderPairs.ToDictionary(p => p.PathOriginalInEpub, p => p.PathAfterProcess);
                if (navigationRefs != null)
                {
                    Action<List<EpubNavigationItemRef>, ObservableCollection<NavigationNestItem>> recursiveNavHandler = default;
                    recursiveNavHandler = async (List<EpubNavigationItemRef> navRefs,ObservableCollection<NavigationNestItem>navItems) =>
                    {
                        foreach (var navRef in navRefs)
                        {
                            string header = navRef.Title;
                            NavigaionLink? nlink=null;
                            
                            if (navRef.Link != null)
                            {
                                var link = navRef.Link;string docPath;
                                if (!htmlPathDict.ContainsKey(link.ContentFilePath))
                                {
                                    var html = localHtmlByPath[link.ContentFilePath];
                                    var (doc, dic) = await HtmlPreprocessAndCacheAsync(html, cachePathBook);
                                    docPath = Path.Combine(cachePathPreprocessed, $"{index++}.html");
                                    using (var fileStream = new FileStream(docPath, FileMode.Create, FileAccess.Write))
                                    {
                                        doc.Save(fileStream);
                                    }
                                    htmlPathDict[link.ContentFilePath] = docPath;
                                }
                                else docPath = htmlPathDict[link.ContentFilePath];
                                nlink = new NavigaionLink
                                {
                                    PathAfterProcess = docPath,
                                    PathOriginalInEpub = link.ContentFilePath,
                                    Anchor = link.Anchor
                                };

                            }
                            if (navRef.HtmlContentFileRef != null)
                            {
                                var htmlRef = navRef.HtmlContentFileRef;string docPath;
                                if (!htmlPathDict.ContainsKey(htmlRef.FilePath))
                                {
                                    var html = localHtmlByPath[htmlRef.FilePath];
                                    var (doc, dic) = await HtmlPreprocessAndCacheAsync(html, cachePathBook);
                                    docPath = Path.Combine(cachePathPreprocessed, $"{index++}.html");
                                    using (var fileStream = new FileStream(docPath, FileMode.Create, FileAccess.Write))
                                    {
                                        doc.Save(fileStream);
                                    }
                                    htmlPathDict[htmlRef.FilePath] = docPath;
                                }
                                else docPath = htmlPathDict[htmlRef.FilePath];
                                if (nlink == null)
                                {
                                    nlink = new NavigaionLink
                                    {
                                        PathAfterProcess = docPath,
                                        PathOriginalInEpub = htmlRef.FilePath,
                                    };
                                }
                            }
                            var navItem = new NavigationNestItem
                            {
                                Header = header,
                                Link = nlink,
                            };
                            navItems.Add(navItem);
                            recursiveNavHandler(navRef.NestedItems,navItem.NestItems);
                            foreach(var subNav in navItem.NestItems)
                            {
                                subNav.Parent = navItem;
                            }
                        }
                    };
                    recursiveNavHandler(navigationRefs, navigationItems);
                }
                else
                {
                    foreach(var it in readingOrderPairs)
                    {
                        navigationItems.Add(
                            new NavigationNestItem
                            {
                                Header = Path.GetFileNameWithoutExtension(it.PathOriginalInEpub),
                                Link = new NavigaionLink
                                {
                                    PathAfterProcess = it.PathAfterProcess,
                                    PathOriginalInEpub = it.PathOriginalInEpub,
                                },
                            }
                        );
                    }
                }


                File.WriteAllText(
                    Path.Combine(cachePathPreprocessed, PreprocessedReadingOrderListName),
                    JsonSerializer.Serialize(readingOrderPairs)
                    );
                File.WriteAllText(
                    Path.Combine(cachePathPreprocessed, PreprocessedResourceCacheDictName),
                    JsonSerializer.Serialize(resourceCachePath)
                    );
                File.WriteAllText(
                    Path.Combine(cachePathPreprocessed, PreprocessedNavigationItemListName),
                    JsonSerializer.Serialize(navigationItems, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve })
                    );
                File.WriteAllText(
                    Path.Combine(cachePathPreprocessed, PreprocessedHtmlPathDictName),
                    JsonSerializer.Serialize(htmlPathDict)
                    );
            }
            catch (Exception ex)
            {
                Directory.Delete(cachePathPreprocessed, true );
                throw;
            }
            
        }
        private static async Task<EpubLocalTextContentFile> ReadLocalTextContentFileAsync(EpubLocalTextContentFileRef localTextContentFileRef)
        {
            string key = localTextContentFileRef.Key;
            EpubContentType contentType = localTextContentFileRef.ContentType;
            string contentMimeType = localTextContentFileRef.ContentMimeType;
            string filePath = localTextContentFileRef.FilePath;
            string content = await localTextContentFileRef.ReadContentAsTextAsync().ConfigureAwait(false);
            return new(key, contentType, contentMimeType, filePath, content);
        }
        private static async Task<ReadOnlyCollection<EpubLocalTextContentFile>> ReadLocalTextContentFilesAsync(ReadOnlyCollection<EpubLocalTextContentFileRef> localTextContentFileRefs)
        {
            List<EpubLocalTextContentFile> localText = new();

            foreach (EpubLocalTextContentFileRef localTextContentFileRef in localTextContentFileRefs)
            {
                localText.Add(await ReadLocalTextContentFileAsync(localTextContentFileRef).ConfigureAwait(false));
            }
            return localText.AsReadOnly();
        }












        
    }
}
