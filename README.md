		
Cruncher
=========

A CSS, Less, Sass, JavaScript, and CoffeeScript Preprocessor for ASP.NET.

[![Build status](https://ci.appveyor.com/api/projects/status/57f2i9iij1i5nun9?svg=true)](https://ci.appveyor.com/project/JamesSouth/cruncher)

Cruncher makes optimizing your resources easy. It can bundle unlimited combinations of remote and local CSS, Less, Sass, JavaScript, and CoffeeScript files.

Combining them, minifying them, and caching them in the browser, Cruncher can handle nested CSS @import statements, re-maps relative resource urls and has a self cleaning cache 
should any changes be made to any of the referenced files. It can also parse CSS and add vendor prefixes to rules by Can I Use using AutoPrefixer. 

Requires 64 bit functionality due to the Sass compiler dependency. If using IIS Express ensure you are running in 64 bit mode.

    Tools > Options > Projects and Solutions > Web Projects > Use the 64 bit version of IIS Express…

Also requires `msvcp110.dll` and `msvcr110.dll` from the Visual C++ Redistributable Package to be installed on the server to support the embedded JavaScript engine.

http://www.microsoft.com/en-us/download/details.aspx?id=40784

If you use Cruncher please get in touch on my twitter [@james_m_south](https://twitter.com/james_m_south). Feedback is always welcome.


Installation.
===============================
Installation is simple. A Nuget package is available [here][1]. 

  [1]: https://nuget.org/packages/Cruncher/


Cruncher Web.config Explained.
===============================

``` xml
<cruncher>
<!-- Whether to allow remote downloads -->
<!-- The maximum allowed remote file size in bytes -->
<!-- The remote file download timeout in milliseconds -->
<security allowRemoteDownloads="true" maxBytes="524288" timeout="300000">
	<!--
	A list of white-listed urls from which we are allowed to download and process remote files.
	The token value allows us to add the file reference without the risk of hitting IEs 1024 
	character url limit.
	-->
	<whiteList>
	<add token="jquery-2.1.1" url="http://ajax.googleapis.com/ajax/libs/jquery/2.1.1/jquery.js" />
	</whiteList>
</security>
<!--
The javascript engine for processing embedded javascript based processors.
Tested working engines include V8JsEngine and MsieJsEngine.
-->
<processing jsEngine="V8JsEngine">
	<!-- The comma separated virtual paths to the css and js folders.-->
	<virtualPaths cssPaths="~/css, ~/content" jsPaths="~/js, ~/scripts" />
	<!-- Where to store crunched files in the application and how long to keep expired ones.-->
	<physicalFiles path="~/assets-cruncher" daysBeforeRemoveExpired="7" />
	<!--
	Autoprefixer options
		- Whether to enable the prefixer.
		- What browsers to support. 
		- Whether to create nice visual cascade of prefixes.
		- Whether to enable the special safe mode to parse broken CSS.
	-->
	<autoPrefixer enabled="true" browsers="> 1%, last 2 versions, Firefox ESR, Opera 12.1" cascade="true" safe="false" />
</processing>
</cruncher>
```

Cruncher Razor Helpers.
=======================

Cruncher comes with some helpers to make adding methods easier.

Simply add `@using Cruncher` to your view and use the following methods to add resources.

``` csharp
// Default.
@CruncherBundler.RenderCSS("style.css", "style.less", "style.scss")

// Render the link wth a media query.
@CruncherBundler.RenderCSS(new HtmlString("media=\"(max-width: 800px)\""), "style.css", "style.less", "style.scss")

// Default.
@CruncherBundler.RenderJavaScript("jquery-2.1.1", "test.coffee", "test.js")

// Render the script with the 'async' boolean enabled.
@CruncherBundler.RenderJavaScript(JavaScriptLoadBehaviour.Async, "jquery-2.1.1", "test.coffee", "test.js")

// Render the script with the 'defer' boolean enabled.
@CruncherBundler.RenderJavaScript(JavaScriptLoadBehaviour.Defer, "jquery-2.1.1", "test.coffee", "test.js")
```
