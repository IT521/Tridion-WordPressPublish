HintTech.eXtensions library
Copyright (c) 2013, Terry S. Francis <terry.francis@hinttech.com>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

1. Following instructions in post: http://tridiondemo.wordpress.com/2013/03/04/part-1-post-components-to-wordpress
2. Following instructions in post: http://tridiondemo.wordpress.com/2013/03/04/part-2-post-components-to-wordpress

3. Unzip the "WordPress Publishing" folder to your eXtensions folder
4. Unzip the "HintTech.eXtensions" folder to your Visual Studio projects folder
5. Update the "WordPress Publishing\WPP.Editor\WordPressPublish.bat" folder with the correct paths
6. Update the system.config file under "<Tridion_home>\web\WebUI\WebRoot\Configuration" with the contents of the System.config.txt file: locate
   the "<editors>" element and add the new "editor" for "WordPress Publishing"

7. In Visual Studio, ensure the Post Build Event path points to wherever you place the "WordPress Publishing\WPP.Editor\WordPressPublish.bat" file
8. In Visual Studio, update the Web.config file with correct values for XXXXXXX
9. Build Solution (Build is signed with Key File "HintTech_eXtensions_Key.pfx", password: Tr1d10n)

10. In SDL Tridion, create a "Blog Post" Schema with the following five fields and one metadata field (see BlogPost.xsd):
	<tcm:Label ElementName="title" Metadata="false">Post Title</tcm:Label>
	<tcm:Label ElementName="bodytext" Metadata="false">Post Body</tcm:Label>
	<tcm:Label ElementName="author" Metadata="false">About the Author</tcm:Label>
	<tcm:Label ElementName="categories" Metadata="false">Post Categories</tcm:Label>
	<tcm:Label ElementName="tags" Metadata="false">Post Tags</tcm:Label>
	
	<tcm:Label ElementName="blogpostid" Metadata="true">Id of this post on Wordpress (Do not modify. It is used to unpublish this post!)</tcm:Label>
	
11. In SDL Tridion, create a "Blog Post Author" Schema with the following fields (see BlogPostAuthor.xsd):
	<tcm:Label ElementName="author" Metadata="false">Blog Post Author</tcm:Label>
	
12. In SDL Tridion, create a "Blog Post" Template Building Block to display your Component in Preview (see BlogPost_TBB.txt for an example using a Razor Template)
13. In SDL Tridion, create a Component Template based on the "Blog Post" Schema and "Blog Post" Template Building Block
14. In SDL Tridion, create a "Post" Component based on the "Blog Post" Schema
15. Right-click on the Post component and select your new GUI eXtension Publishing->Publish (WordPress)