/* 
HintTech.eXtensions library
Copyright (c) 2013, Terry S. Francis <terry.francis@hinttech.com>

Permission is hereby granted, free of charge, to any person 
obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, 
and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be 
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CookComputing.XmlRpc;
using Tridion.ContentManager.CoreService.Client;

namespace HintTech.eXtensions
{
    public partial class WordPressPublishBase : System.Web.UI.Page
    {
        #region Global Variables
        SessionAwareCoreServiceClient client;

        // API settings (Read from Web.Config)
        private string urlXMLRPC;
        private string blogUserName;
        private string blogPassWord;
        private string postSchemaId;        // TCM URI of Blog Post schema
        private string tcmUserName;
        private string tcmPassWord;
        private string errorLog;
        private string tcpEndpoint;
        private string httpEndpoint;

        private int blogId;

        IWordPressApi myBlog;

        // Error log
        protected StreamWriter log;
        #endregion

        virtual protected void Page_Load(object sender, EventArgs e)
        {
            // API settings (Read from Web.Config)
            urlXMLRPC = ConfigurationManager.AppSettings["urlXMLRPC"].ToString();
            blogUserName = ConfigurationManager.AppSettings["blogUserName"].ToString();
            blogPassWord = ConfigurationManager.AppSettings["blogPassWord"].ToString();
            postSchemaId = ConfigurationManager.AppSettings["postSchemaId"].ToString();
            tcmUserName = ConfigurationManager.AppSettings["tcmUserName"].ToString();
            tcmPassWord = ConfigurationManager.AppSettings["tcmPassWord"].ToString();
            errorLog = ConfigurationManager.AppSettings["errorLog"].ToString();
            tcpEndpoint = ConfigurationManager.AppSettings["tcpEndpoint"].ToString();
            httpEndpoint = ConfigurationManager.AppSettings["httpEndpoint"].ToString();

            // Create a writer and open the file:
            log = File.CreateText(errorLog);
            log.WriteLine("***** PostToWordPress: {0} *****", DateTime.Now.ToString("f"));

            // Create a reference to Core Service
            NetTcpBinding binding = new NetTcpBinding();
            binding.Name = "netTcp";
            binding.TransactionFlow = true;
            binding.TransactionProtocol = TransactionProtocol.WSAtomicTransaction11;
            binding.MaxReceivedMessageSize = 10485760;
            binding.ReaderQuotas.MaxStringContentLength = 10485760;
            binding.ReaderQuotas.MaxArrayLength = 10485760;
            EndpointAddress endpoint = new EndpointAddress(tcpEndpoint);
            client = new SessionAwareCoreServiceClient(binding, endpoint);
            client.ChannelFactory.Credentials.Windows.ClientCredential = new NetworkCredential(tcmUserName, tcmPassWord);

            // Get user's blog id
            blogId = GetBlogID();

            // Initialize interface
            myBlog = XmlRpcProxyGen.Create<IWordPressApi>();
            myBlog.Url = urlXMLRPC;
        }

        protected PostResult PostToWordPress(string componentId)
        {
            ComponentData component = client.Read(componentId, null) as ComponentData;

            if (component.Schema.IdRef == postSchemaId)
            {
                // Load content in a XDocument
                XDocument xDoc = XDocument.Parse(component.Content);

                // Get content namespace
                XNamespace ns = xDoc.Root.Attribute("xmlns").Value;

                // Copy component to post
                Post newBlogPost = default(Post);

                newBlogPost.Type = "post";
                newBlogPost.Status = "publish";
                newBlogPost.Name = component.Title.Replace("-", "").Replace("  ", " ").Replace(" ","-").ToLower();
                newBlogPost.Title = xDoc.Root.Element(ns + "title").Value;
                newBlogPost.Content = ReplaceImagesInContent(xDoc.Root.Element(ns + "bodytext")) + 
                    GetAuthorContent(xDoc.Root.Element(ns + "author"));

                bool addTaxonomy = false;
                XmlRpcStruct taxonomy = new XmlRpcStruct();
                List<string> taxNames = new List<string>();
                foreach (XElement name in xDoc.Root.Elements(ns + "categories")) {
                    taxNames.Add(name.Value);
                }
                if (taxNames.Count > 0)
                {
                    taxonomy.Add("category", taxNames.ToArray());
                    taxNames.Clear();
                    addTaxonomy = true;
                }
                foreach (XElement name in xDoc.Root.Elements(ns + "tags"))
                {
                    taxNames.Add(name.Value);
                }
                if (taxNames.Count > 0)
                {
                    taxonomy.Add("post_tag", taxNames.ToArray());
                    addTaxonomy = true;
                }
                if (addTaxonomy) {
                    newBlogPost.TermsNames = taxonomy;
                }

                // Post component
                return PostComponent(newBlogPost, component);
            }
            else
            {
                PostResult postResult = new PostResult();
                postResult.message = "Component not based on correct schema! Component: " + component.Title + " (" + component.Id + ")\n";
                postResult.result = false;
                return postResult;
            }
        }

        /// <summary>
        /// Gets ID of first blog for user
        /// </summary>
        /// <returns></returns>
        private int GetBlogID()
        {
            int blogID = 1;
            try
            {
                // Get first id returned and convert to integer
                UserBlog[] userBlogs = myBlog.GetUsersBlogs(blogUserName, blogPassWord);
                log.WriteLine("userBlogs.Length: {0}", userBlogs.Length);
                if (userBlogs.Length > 0)
                {
                    blogID = Convert.ToInt32(userBlogs[0].blogId);
                    log.WriteLine("blogID: {0}", blogID);
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("GetBlogID ERROR: " + ex.Message);
            }

            return blogID;
        }

        /// <summary>
        /// Get component linked by author field and return its content
        /// </summary>
        /// <param name="author"></param>
        /// <returns></returns>
        private string GetAuthorContent(XElement author)
        {
            string returnValue = "";
            try
            {
                XNamespace xlink = "http://www.w3.org/1999/xlink";
                ComponentData authorComponent = client.Read(author.Attribute(xlink + "href").Value, null) as ComponentData;

                // Load new content in a XDocument
                XDocument xDoc = XDocument.Parse(authorComponent.Content);

                // Get new content namespace
                XNamespace ns = xDoc.Root.Attribute("xmlns").Value;

                returnValue = xDoc.Root.Element(ns + "author").ToString();
            }
            catch (Exception ex) {
                log.WriteLine("GetAuthorContent ERROR: " + ex.Message);
            }
            return returnValue;
        }

        /// <summary>
        /// Finds links to images in the content (RTF Field) and
        /// uploads the images to WordPress and adjust the link 
        /// so the img src points to the newly uploaded image
        /// </summary>
        /// <param name="content">The content to search images in</param>
        /// <returns>The adjusted content (img src points to WordPress location)</returns>
        private string ReplaceImagesInContent(XElement content)
        {
            string xsd = content.ToString();

            try
            {
                // Replace image href attribute with src attribute
                Regex rgx = new Regex("img xlink:href=\"tcm:[^\"]+?\"", RegexOptions.IgnoreCase);
                MatchCollection matches = rgx.Matches(xsd);
                if (matches.Count > 0)
                {
                    Match[] tempArray = new Match[matches.Count];
                    matches.CopyTo(tempArray, 0);
                    List<Match> distinct = tempArray.Distinct().ToList();

                    foreach (Match match in distinct)
                    {
                        string src = string.Format("img src=\"{0}\"", UploadImage(match.Value.Replace("img xlink:href=", "").Replace("\"", "")));
                        xsd = xsd.Replace(match.Value, src);
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteLine("ReplaceImagesInContent replace attribute ERROR: " + ex.Message);
            }

            try { 
                // Delete Tridion specific attributes
                xsd = RegexReplace(" xlink:title=\"[^\"]+?\"", xsd, "");
                xsd = RegexReplace(" xmlns:xlink=\"[^\"]+?\"", xsd, "");
            }
            catch (Exception ex)
            {
                log.WriteLine("ReplaceImagesInContent delete attribute ERROR: " + ex.Message);
            }

            content.SetValue(xsd);

            return content.Value;
        }

        /// <summary>
        /// Use Regex to replace one set of characters in source string
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="source"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private string RegexReplace(string pattern, string source, string replacement) {
            Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection matches = rgx.Matches(source);
            if (matches.Count > 0)
            {
                Match[] tempArray = new Match[matches.Count];
                matches.CopyTo(tempArray, 0);
                List<Match> distinct = tempArray.Distinct().ToList();

                foreach (Match match in distinct)
                {
                    source = source.Replace(match.Value, replacement);
                }
            }
            return source;
        }

        /// <summary>
        /// Upload image to WordPress.
        /// </summary>
        /// <param name="tcmUri">The uri of the multimedia component to upload</param>
        /// <returns>A url to the uploaded image</returns>
        private string UploadImage(string tcmUri)
        {
            MediaObjectUrl UrlToNewMedia = new MediaObjectUrl();
            UrlToNewMedia.url = "";
            try
            {
                ComponentData binaryItem = client.Read(tcmUri, null) as ComponentData;

                MediaObject myObject = new MediaObject();
                myObject.name = binaryItem.BinaryContent.Filename.ToString();
                myObject.type = binaryItem.BinaryContent.MimeType;

                // Copy the existing file from Tridion
                byte[] binaryContent = null;

                if (binaryItem.BinaryContent.FileSize != -1)
                {
                    binaryContent = GetMultimediaAsStream(tcmUri);
                }
                myObject.bits = binaryContent;
                myObject.overwrite = true;

                //Upload            
                UrlToNewMedia = myBlog.UploadFile(blogId, blogUserName, blogPassWord, myObject);
            }
            catch (Exception ex)
            {
                log.WriteLine("UploadImage ERROR: " + ex.Message);
            }

            //Grab Url of newly uploaded image...
            return UrlToNewMedia.url;
        }

        /// <summary>
        /// Get Multimedia Component binary As Stream
        /// </summary>
        /// <param name="tcmUri"></param>
        /// <returns></returns>
        public byte[] GetMultimediaAsStream(string tcmUri)
        {
            // Binding and endpoint needed for streaming (UploadImage)
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "streamDownload_basicHttp";
            binding.MaxReceivedMessageSize = 209715200;
            binding.TransferMode = TransferMode.StreamedResponse;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            EndpointAddress endpoint = new EndpointAddress(httpEndpoint);
            StreamDownloadClient streamDownloadClient = new StreamDownloadClient(binding, endpoint);
            streamDownloadClient.ChannelFactory.Credentials.Windows.ClientCredential = new NetworkCredential(tcmUserName, tcmPassWord);

            byte[] returnValue = new byte[] { };
            try
            {
                Stream tempStream = streamDownloadClient.DownloadBinaryContent(tcmUri);
                var memoryStream = new MemoryStream();
                tempStream.CopyTo(memoryStream);
                returnValue = memoryStream.ToArray();

            }
            catch (Exception ex)
            {
                log.WriteLine("GetMultimediaAsStream ERROR: " + ex.Message);
            }
            
            return returnValue;
        }

        /// <summary>
        /// Post Component to WordPress
        /// </summary>
        /// <param name="newBlogPost"></param>
        /// <returns></returns>
        private PostResult PostComponent(Post newBlogPost, ComponentData component)
        {
            try
            {
                // Remove Tridion specific elements and attributes
                newBlogPost.Content = RegexReplace(@"<bodytext\b[^>]*>", newBlogPost.Content, "");
                newBlogPost.Content = RegexReplace(@"<author\b[^>]*>", newBlogPost.Content, "");
                newBlogPost.Content = RegexReplace(@"</bodytext>", newBlogPost.Content, "");
                newBlogPost.Content = RegexReplace(@"</author>", newBlogPost.Content, "");
                newBlogPost.Content = RegexReplace(@" xmlns=""[^""]+?""", newBlogPost.Content, "");
            }
            catch (Exception ex)
            {
                log.WriteLine("PostComponent clean content ERROR: " + ex.Message);
            }

            PostResult postResult = new PostResult();
            postResult.message = "Component: " + component.Title + " (" + component.Id + ") ";

            try
            {
                string publishedPostID = myBlog.NewPost(blogId, blogUserName, blogPassWord, newBlogPost);

                try {
                    // Load metadata in a XDocument
                    XDocument xDoc = XDocument.Parse(component.Metadata);

                    // Get metadata namespace
                    XNamespace ns = xDoc.Root.Attribute("xmlns").Value;
                    
                    // Get field called blogpostid
                    XElement blogpostid = xDoc.Root.Element(ns + "blogpostid");
                    if (blogpostid != null)
                    {
                        // Save published post ID in component metadata
                        blogpostid.Value = publishedPostID;
                    }
                    else {
                        // Add new blogpostid element
                        if (xDoc.Root.HasElements)
                        {
                            xDoc.Root.Elements().Last().AddAfterSelf(new XElement((XName)(ns + "blogpostid")));
                        }
                        else {
                            xDoc.Root.AddFirst(new XElement((XName)(ns + "blogpostid")));
                        }
                        // Save published post ID in component metadata
                        blogpostid = xDoc.Root.Element(ns + "blogpostid");
                        blogpostid.Value = publishedPostID;
                    }
                    component.Metadata = xDoc.Root.ToString();
                    client.Update(component, null);
                }
                catch (Exception ex) {
                    log.WriteLine("PostComponent Metadata ERROR: " + ex.Message);
                }

                postResult.message += "posted to Blog successfullly! Post ID: " + publishedPostID + "\n";
                postResult.result = true;
                return postResult;
            }
            catch (XmlRpcFaultException ex)
            {
                log.WriteLine("PostComponent XmlRpcFaultException ERROR: " + ex.Message);
                postResult.message += "NOT posted to Blog!\n";
                postResult.result = false;
                return postResult;
            }
        }
    }

    public partial class WordPressPublish : WordPressPublishBase
    {
        protected override void Page_Load(object sender, EventArgs e)
        {
            StringBuilder result = new StringBuilder();

            try
            {
                base.Page_Load(sender, e);

                // Get input parameters: Component TCM URI[, Component TCM URI, ...]
                string args = Request.QueryString["componentIds"];
                string[] componentIds = args.Split(',');
                foreach (string componentId in componentIds)
                {
                    // Post each component
                    result.Append(PostToWordPress(componentId).message);
                }
            }
            catch(Exception ex) {
                log.WriteLine("Page_Load ERROR: " + ex.Message);
            }
            finally
            {
                // Close the error log stream
                log.Close();

                // Return text with single quotes escaped to calling JavaScript program
                Response.Write(result.ToString().Replace("'", "\'"));
            }
        }
    }
}