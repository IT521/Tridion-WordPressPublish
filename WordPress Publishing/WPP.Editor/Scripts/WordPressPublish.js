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

Type.registerNamespace("HintTech.eXtensions");

HintTech.eXtensions.WordPressPublish = function Commands$WordPressPublish() {
    Type.enableInterface(this, "HintTech.eXtensions.WordPressPublish");
    this.addInterface("Tridion.Cme.Command", ["WordPressPublish"]);
};

HintTech.eXtensions.WordPressPublish.prototype._isAvailable = function WordPressPublish$_isAvailable(selection) {
    return true;
};

HintTech.eXtensions.WordPressPublish.prototype._isEnabled = function WordPressPublish$_isEnabled(selection) {
    if (selection.getItems().length < 1)
	    return false;
    else
	    return true;
};

HintTech.eXtensions.WordPressPublish.prototype._execute = function WordPressPublish$_execute(selection) {
    if (selection) {
        var items = selection.getItems();
        if (items && items.length > 0) {
            var urlWPP = "/WebUI/Editors/WordPressPublish/WordPressPublish.aspx";
            // Call WordPressPublish.aspx with selected component IDs separated by commas
            // Response from WordPressPublish.aspx is output via an alert
            jQuery.get(urlWPP, { componentIds: "{0}".format(items) }, function (data) { alert(data); }, "text");
        }
    }
};