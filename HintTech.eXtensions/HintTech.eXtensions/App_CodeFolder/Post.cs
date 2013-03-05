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
using CookComputing.XmlRpc;

namespace HintTech.eXtensions
{
    [XmlRpcUrl("http://tridiondemo.wordpress.com/xmlrpc.php")]
    public interface IWordPressApi : IXmlRpcProxy
    {
        [XmlRpcMethod("wp.newPost")]
        string NewPost(int blogid, string username, string password, Post post);

        [XmlRpcMethod("wp.uploadFile")]
        MediaObjectUrl UploadFile(int blogid, string username, string password, MediaObject data);

        [XmlRpcMethod("wp.getUsersBlogs")]
        UserBlog[] GetUsersBlogs(string username, string password);
    }

    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Inclosure
    {
        public string Url { get; set; }
        public int Length { get; set; }
        public string Type { get; set; }
    }

    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MediaObject
    {
        [XmlRpcMember("name")]
        public string name;     // Filename

        [XmlRpcMember("type")]
        public string type;     // File MIME type

        [XmlRpcMember("bits")]
        public byte[] bits;     // base64-encoded binary data

        [XmlRpcMember("overwrite")]
        public bool overwrite;  // Optional. Overwrite an existing attachment of the same name
    }

    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MediaObjectUrl
    {
        [XmlRpcMember("id")]
        public string id;

        [XmlRpcMember("file")]
        public string file;

        [XmlRpcMember("url")]
        public string url;

        [XmlRpcMember("type")]
        public string type;
    }

    /// <summary> 
    /// This struct represents information about a user's blog. 
    /// </summary> 
    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct UserBlog
    {
        [XmlRpcMember("blogId")]
        public string blogId;

        [XmlRpcMember("blogName")]
        public string blogName;

        [XmlRpcMember("url")]
        public string url;

        [XmlRpcMember("xmlrpc")]
        public string xmlrpc;       // XML-RPC endpoint for the blog

        [XmlRpcMember("isAdmin")]
        public bool isAdmin;
    }

    [Serializable]
    [XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct Post
    {
        // [ 'post' | 'page' | 'link' | 'nav_menu_item' | custom post type ] 
        [XmlRpcMember("post_type")]
        public string Type { get; set; }                // You may want to insert a regular post, page, link, a menu item or some custom post type

        // [ 'draft' | 'publish' | 'pending'| 'future' | 'private' | custom registered status ]
        [XmlRpcMember("post_status")]
        public string Status { get; set; }              // Set the status of the new post.

        // [ <the title> ]
        [XmlRpcMember("post_title")]
        public string Title { get; set; }               // The title of your post.

        // [ <user ID number> ]
        [XmlRpcMember("post_author")]
        public int? Author { get; set; }                 // The user ID number of the author.

        // [ <an excerpt> ]
        [XmlRpcMember("post_excerpt")]
        public string Excerpt { get; set; }             // For all your post excerpt needs.

        // [ <the text of the post> ]
        [XmlRpcMember("post_content")]
        public string Content { get; set; }             // The full text of the post.

        // [ Y-m-d H:i:s ]
        [XmlRpcMember("post_date")]
        public DateTime? Date { get; set; }              // The time post was made.

        [XmlRpcMember("post_format")]
        public string Format { get; set; }

        // [ <the name> ]
        [XmlRpcMember("post_name")]
        public string Name { get; set; }                // The name (slug) for your post

        // [ ? ]
        [XmlRpcMember("post_password")]
        public string Password { get; set; }            // Password for post?

        // [ 'closed' | 'open' ]
        [XmlRpcMember("comment_status")]
        public string CommentStatus { get; set; }       // 'closed' means no comments.

        // [ 'closed' | 'open' ]
        [XmlRpcMember("ping_status")]
        public string PingStatus { get; set; }          // 'closed' means pingbacks or trackbacks turned off

        [XmlRpcMember("sticky")]
        public bool Sticky { get; set; }

        [XmlRpcMember("post_thumbnail")]
        public int? Thumbnail { get; set; }

        // [ <post ID> ]
        [XmlRpcMember("post_parent")]
        public int? Parent { get; set; }                 // Sets the parent of the new post.

        [XmlRpcMember("custom_fields")]
        public XmlRpcStruct CustomFields { get; set; }

        [XmlRpcMember("terms")]
        public XmlRpcStruct Terms { get; set; }             // Taxonomy names as keys, array of term IDs as values.

        [XmlRpcMember("terms_names")]
        public XmlRpcStruct TermsNames { get; set; }        // Taxonomy names as keys, array of term names as values.

        [XmlRpcMember("enclosure")]
        public Inclosure? Enclosure { get; set; }
    }

    public class PostResult {
        public bool result { get; set; }
        public string message { get; set; }
    }
}