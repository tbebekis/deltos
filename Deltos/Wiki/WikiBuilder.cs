// Copyright (c) 2026 Theodoros Bebekis
// Licensed under the MIT License.

namespace Deltos.Wiki;

/// <summary>
/// Builds a static component wiki.
/// </summary>
static public class WikiBuilder
{
    // ● private
    /// <summary>
    /// Combines path parts.
    /// </summary>
    /// <param name="Parts">The path parts.</param>
    /// <returns>The combined path.</returns>
    static string P(params string[] Parts) => System.IO.Path.Combine(Parts);
    /// <summary>
    /// Writes a log line.
    /// </summary>
    /// <param name="Result">The build result.</param>
    /// <param name="Text">The log text.</param>
    static void LogLine(WikiBuildResult Result, string Text)
    {
        Result?.Log.Add(Text);
    }
    /// <summary>
    /// Adds an emitted file.
    /// </summary>
    /// <param name="Result">The build result.</param>
    /// <param name="RelativePath">The relative path.</param>
    static void AddEmitted(WikiBuildResult Result, string RelativePath)
    {
        Result?.EmittedFiles.Add(RelativePath.Replace('\\', '/'));
    }
    /// <summary>
    /// Returns HTML escaped text.
    /// </summary>
    /// <param name="Text">The source text.</param>
    /// <returns>The escaped text.</returns>
    static string HtmlEscape(string Text) => WebUtility.HtmlEncode(Text ?? string.Empty);
    /// <summary>
    /// Returns a slug for text.
    /// </summary>
    /// <param name="Text">The source text.</param>
    /// <returns>The slug.</returns>
    static string Slug(string Text)
    {
        if (string.IsNullOrWhiteSpace(Text))
            return string.Empty;

        string Lower = Text.ToLowerInvariant();
        StringBuilder Builder = new();
        foreach (char Char in Lower)
        {
            if (Char >= 'a' && Char <= 'z' || Char >= '0' && Char <= '9')
                Builder.Append(Char);
            else if (Char == ' ' || Char == '_' || Char == '-' || Char == '.')
                Builder.Append('-');
        }

        string Result = Regex.Replace(Builder.ToString(), "-+", "-").Trim('-');
        return Result;
    }
    /// <summary>
    /// Normalizes new lines.
    /// </summary>
    /// <param name="Text">The source text.</param>
    /// <returns>The normalized text.</returns>
    static string NormalizeNewlines(string Text)
    {
        return (Text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
    }
    /// <summary>
    /// Returns text tail.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="MaxChars">The maximum character count.</param>
    /// <returns>The tail text.</returns>
    static string GetTail(string Text, int MaxChars)
    {
        Text ??= string.Empty;
        return Text.Length <= MaxChars ? Text : Text.Substring(Text.Length - MaxChars);
    }
    /// <summary>
    /// Safely cleans the output folder while preserving git metadata.
    /// </summary>
    /// <param name="FolderPath">The output folder path.</param>
    /// <param name="Result">The build result.</param>
    static void SafeCleanOutputFolder(string FolderPath, WikiBuildResult Result)
    {
        if (string.IsNullOrWhiteSpace(FolderPath))
            return;

        if (!System.IO.Directory.Exists(FolderPath))
        {
            System.IO.Directory.CreateDirectory(FolderPath);
            LogLine(Result, $"Output folder created: {FolderPath}");
            return;
        }

        HashSet<string> KeepNames = new(StringComparer.OrdinalIgnoreCase) { ".git", ".github", ".gitignore", ".gitattributes" };
        foreach (string ItemPath in System.IO.Directory.EnumerateFileSystemEntries(FolderPath))
        {
            string Name = System.IO.Path.GetFileName(ItemPath);
            if (KeepNames.Contains(Name))
                continue;

            if (System.IO.Directory.Exists(ItemPath))
                System.IO.Directory.Delete(ItemPath, true);
            else
                System.IO.File.Delete(ItemPath);
        }
    }
    /// <summary>
    /// Reads a wiki resource file.
    /// </summary>
    /// <param name="FileName">The resource file name.</param>
    /// <returns>The resource text.</returns>
    static string ReadWikiResource(string FileName)
    {
        string FilePath = P(AppContext.BaseDirectory, "Resources", "Wiki", FileName);
        if (!System.IO.File.Exists(FilePath))
            throw new InvalidOperationException($"Wiki resource file not found: {FilePath}");

        return System.IO.File.ReadAllText(FilePath, Encoding.UTF8);
    }
    /// <summary>
    /// Writes a UTF-8 text file.
    /// </summary>
    /// <param name="FilePath">The file path.</param>
    /// <param name="Text">The text.</param>
    static void WriteText(string FilePath, string Text)
    {
        string FolderPath = System.IO.Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrWhiteSpace(FolderPath))
            System.IO.Directory.CreateDirectory(FolderPath);

        System.IO.File.WriteAllText(FilePath, Text ?? string.Empty, Encoding.UTF8);
    }
    /// <summary>
    /// Writes wiki assets.
    /// </summary>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="Result">The build result.</param>
    static void WriteAssets(string OutputFolder, WikiBuildResult Result)
    {
        WriteText(P(OutputFolder, "assets", "css", "wiki.css"), ReadWikiResource("wiki.css"));
        WriteText(P(OutputFolder, "assets", "js", "wiki.js"), ReadWikiResource("wiki.js"));
        AddEmitted(Result, "assets/css/wiki.css");
        AddEmitted(Result, "assets/js/wiki.js");
    }
    /// <summary>
    /// Copies project images to wiki assets.
    /// </summary>
    /// <param name="Project">The project.</param>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="Result">The build result.</param>
    static void CopyImages(Project Project, string OutputFolder, WikiBuildResult Result)
    {
        string SourceFolder = Project.ImagesFolderPath;
        string DestFolder = P(OutputFolder, "assets", "images");
        if (!System.IO.Directory.Exists(SourceFolder))
        {
            LogLine(Result, $"Images folder not found: {SourceFolder}");
            return;
        }

        foreach (string SourcePath in System.IO.Directory.GetFiles(SourceFolder, "*", System.IO.SearchOption.AllDirectories))
        {
            string RelativePath = System.IO.Path.GetRelativePath(SourceFolder, SourcePath);
            string DestPath = P(DestFolder, RelativePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(DestPath));
            System.IO.File.Copy(SourcePath, DestPath, true);
            AddEmitted(Result, "assets/images/" + RelativePath.Replace('\\', '/'));
        }
    }
    /// <summary>
    /// Collects component information.
    /// </summary>
    /// <param name="Info">The build info.</param>
    /// <returns>The component info list.</returns>
    static List<WikiComponentInfo> CollectComponents(WikiBuildInfo Info)
    {
        List<WikiComponentInfo> Result = new();
        foreach (Component Component in Info.Project.Components)
        {
            string Text = Info.UseSecondaryText ? Component.Text2 : Component.Text;
            if (Info.UseSecondaryText && string.IsNullOrWhiteSpace(Text))
                continue;

            WikiComponentInfo Item = new WikiComponentInfo();
            Item.Component = Component;
            Item.Title = Info.UseSecondaryText ? Component.Title2OrTitle : Component.Title;
            Item.Category = Component.Category;
            Item.Text = Text ?? string.Empty;
            Item.AliasList.AddRange(Component.AliasList);
            Item.TagList.AddRange(Component.TagList);
            Result.Add(Item);
        }

        return Result;
    }
    /// <summary>
    /// Builds category map.
    /// </summary>
    /// <param name="Components">The component list.</param>
    /// <returns>The category map.</returns>
    static Dictionary<string, List<WikiComponentInfo>> BuildCategoryMap(List<WikiComponentInfo> Components)
    {
        return Components
            .Where(Item => !string.IsNullOrWhiteSpace(Item.Category))
            .GroupBy(Item => Item.Category, StringComparer.OrdinalIgnoreCase)
            .OrderBy(Item => Item.Key)
            .ToDictionary(Item => Item.Key, Item => Item.OrderBy(Component => Component.Title).ToList(), StringComparer.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Builds tag map.
    /// </summary>
    /// <param name="Components">The component list.</param>
    /// <returns>The tag map.</returns>
    static Dictionary<string, List<WikiComponentInfo>> BuildTagMap(List<WikiComponentInfo> Components)
    {
        Dictionary<string, List<WikiComponentInfo>> Result = new(StringComparer.OrdinalIgnoreCase);
        foreach (WikiComponentInfo Component in Components)
        {
            foreach (string Tag in Component.TagList.Where(Item => !string.IsNullOrWhiteSpace(Item)))
            {
                if (!Result.TryGetValue(Tag, out List<WikiComponentInfo> List))
                {
                    List = new List<WikiComponentInfo>();
                    Result[Tag] = List;
                }

                List.Add(Component);
            }
        }

        return Result.OrderBy(Item => Item.Key).ToDictionary(Item => Item.Key, Item => Item.Value.OrderBy(Component => Component.Title).ToList(), StringComparer.OrdinalIgnoreCase);
    }
    /// <summary>
    /// Builds the term map.
    /// </summary>
    /// <param name="Components">The component list.</param>
    /// <returns>The term map.</returns>
    static Dictionary<string, string> BuildTermMap(List<WikiComponentInfo> Components)
    {
        Dictionary<string, string> Result = new(StringComparer.OrdinalIgnoreCase);
        foreach (WikiComponentInfo Component in Components)
        {
            string Url = "/components/" + Slug(Component.Title) + ".html";
            if (!Result.ContainsKey(Component.Title))
                Result[Component.Title] = Url;

            foreach (string Alias in Component.AliasList.Where(Item => !string.IsNullOrWhiteSpace(Item)))
            {
                if (!Result.ContainsKey(Alias))
                    Result[Alias] = Url;
            }
        }

        return Result;
    }
    /// <summary>
    /// Applies basic markdown preprocessing.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <param name="AllTitles">All component titles.</param>
    /// <returns>The preprocessed markdown.</returns>
    static string PreprocessMarkdown(string MarkdownText, List<string> AllTitles)
    {
        string Result = MarkdownText ?? string.Empty;
        foreach (string Title in AllTitles)
        {
            string FileName = Title.Replace(" ", string.Empty) + ".md";
            string Target = "/components/" + Slug(Title) + ".html";
            Result = Result.Replace("(" + FileName + ")", "(" + Target + ")", StringComparison.OrdinalIgnoreCase);
            Result = Result.Replace("[" + Title + "]()", "[" + Title + "](" + Target + ")", StringComparison.OrdinalIgnoreCase);
        }

        return Result;
    }
    /// <summary>
    /// Auto-links component terms in markdown.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <param name="TermMap">The term map.</param>
    /// <returns>The linked markdown.</returns>
    static string AutoLinkTermsInMarkdown(string MarkdownText, Dictionary<string, string> TermMap)
    {
        if (string.IsNullOrEmpty(MarkdownText) || TermMap.Count == 0)
            return MarkdownText ?? string.Empty;

        List<string> Tokens = new();
        string Work = MarkdownText;
        Work = ProtectMarkdownTokens(Work, Tokens);

        string Pattern = @"(?<![\p{L}\p{N}_])(" + string.Join("|", TermMap.Keys.OrderByDescending(Item => Item.Length).Select(Regex.Escape)) + @")(?![\p{L}\p{N}_])";
        Work = Regex.Replace(Work, Pattern, Match =>
        {
            string Term = Match.Value;
            return TermMap.TryGetValue(Term, out string Url) ? $"[{Term}]({Url})" : Term;
        }, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return RestoreMarkdownTokens(Work, Tokens);
    }
    /// <summary>
    /// Protects markdown tokens from auto-linking.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <param name="Tokens">The protected tokens.</param>
    /// <returns>The protected markdown text.</returns>
    static string ProtectMarkdownTokens(string MarkdownText, List<string> Tokens)
    {
        string Result = MarkdownText ?? string.Empty;
        Result = ProtectPattern(Result, Tokens, @"```[\s\S]*?```");
        Result = ProtectPattern(Result, Tokens, @"~~~[\s\S]*?~~~");
        Result = ProtectPattern(Result, Tokens, @"`[^`\r\n]*`");
        Result = ProtectPattern(Result, Tokens, @"!?\[[^\]\r\n]*\]\([^\)\r\n]*\)");
        return Result;
    }
    /// <summary>
    /// Protects regex matches.
    /// </summary>
    /// <param name="Text">The text.</param>
    /// <param name="Tokens">The token list.</param>
    /// <param name="Pattern">The regex pattern.</param>
    /// <returns>The protected text.</returns>
    static string ProtectPattern(string Text, List<string> Tokens, string Pattern)
    {
        return Regex.Replace(Text, Pattern, Match =>
        {
            string Token = $"@@TOKEN_{Tokens.Count}@@";
            Tokens.Add(Match.Value);
            return Token;
        }, RegexOptions.CultureInvariant);
    }
    /// <summary>
    /// Restores protected markdown tokens.
    /// </summary>
    /// <param name="Text">The protected text.</param>
    /// <param name="Tokens">The token list.</param>
    /// <returns>The restored text.</returns>
    static string RestoreMarkdownTokens(string Text, List<string> Tokens)
    {
        string Result = Text ?? string.Empty;
        for (int Index = 0; Index < Tokens.Count; Index++)
            Result = Result.Replace($"@@TOKEN_{Index}@@", Tokens[Index]);

        return Result;
    }
    /// <summary>
    /// Builds a category markdown link.
    /// </summary>
    /// <param name="Category">The category.</param>
    /// <returns>The category link.</returns>
    static string BuildCategoryLink(string Category, Dictionary<string, List<WikiComponentInfo>> Categories)
    {
        if (string.IsNullOrWhiteSpace(Category))
            return string.Empty;

        string Url = BuildGroupFirstComponentUrl("categories", Category, Categories);
        return $"[{Category}]({Url})";
    }
    /// <summary>
    /// Builds a tag markdown link.
    /// </summary>
    /// <param name="Tag">The tag.</param>
    /// <returns>The tag link.</returns>
    static string BuildTagLink(string Tag, Dictionary<string, List<WikiComponentInfo>> Tags)
    {
        if (string.IsNullOrWhiteSpace(Tag))
            return string.Empty;

        string Url = BuildGroupFirstComponentUrl("tags", Tag, Tags);
        return $"[{Tag}]({Url})";
    }
    /// <summary>
    /// Builds a URL to the first component of a group.
    /// </summary>
    /// <param name="PanelName">The panel name.</param>
    /// <param name="GroupName">The group name.</param>
    /// <param name="Groups">The group map.</param>
    /// <returns>The first component URL.</returns>
    static string BuildGroupFirstComponentUrl(string PanelName, string GroupName, Dictionary<string, List<WikiComponentInfo>> Groups)
    {
        string GroupSlug = Slug(GroupName);
        string GroupKey = PanelName + "-" + GroupSlug;
        if (Groups != null && Groups.TryGetValue(GroupName, out List<WikiComponentInfo> Components) && Components.Count > 0)
        {
            string ItemKey = Slug(Components[0].Title);
            return AddNavQuery("/components/" + ItemKey + ".html", PanelName, GroupKey, ItemKey);
        }

        string FallbackUrl = PanelName == "categories" ? "/categories/" + GroupSlug + ".html" : "/tags/" + GroupSlug + ".html";
        return AddNavQuery(FallbackUrl, PanelName, GroupKey, string.Empty);
    }
    /// <summary>
    /// Appends the taxonomy footer.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <param name="Category">The category.</param>
    /// <param name="Tags">The tags.</param>
    /// <returns>The markdown with footer.</returns>
    static string AppendTaxonomyFooter(string MarkdownText, string Category, List<string> Tags, Dictionary<string, List<WikiComponentInfo>> Categories, Dictionary<string, List<WikiComponentInfo>> TagMap)
    {
        string Norm = NormalizeNewlines(MarkdownText).TrimEnd();
        string Tail = GetTail(Norm, 400);
        StringBuilder Builder = new();
        Builder.Append(Norm);
        Builder.AppendLine();
        Builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(Category) && Tail.IndexOf("Category:", StringComparison.OrdinalIgnoreCase) < 0)
            Builder.AppendLine("Category: " + BuildCategoryLink(Category, Categories));

        string TagsLine = string.Join(", ", Tags.Where(Item => !string.IsNullOrWhiteSpace(Item)).Select(Item => BuildTagLink(Item, TagMap)));
        if (!string.IsNullOrWhiteSpace(TagsLine) && Tail.IndexOf("Tags:", StringComparison.OrdinalIgnoreCase) < 0)
            Builder.AppendLine("Tags: " + TagsLine);

        return Builder.ToString();
    }
    /// <summary>
    /// Renders markdown to HTML.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <returns>The HTML fragment.</returns>
    static string RenderMarkdownToHtml(string MarkdownText)
    {
        string Html = Markdig.Markdown.ToHtml(MarkdownText ?? string.Empty);
        Html = Html.Replace("src=\"../Images/", "src=\"/assets/images/", StringComparison.OrdinalIgnoreCase);
        Html = Html.Replace("src=\"Images/", "src=\"/assets/images/", StringComparison.OrdinalIgnoreCase);
        Html = Html.Replace("src=\"/Images/", "src=\"/assets/images/", StringComparison.OrdinalIgnoreCase);
        return "<article>" + Html + "</article>";
    }
    /// <summary>
    /// Strips markdown to text.
    /// </summary>
    /// <param name="MarkdownText">The markdown text.</param>
    /// <returns>The plain text.</returns>
    static string StripMarkdownToText(string MarkdownText)
    {
        string Result = Regex.Replace(MarkdownText ?? string.Empty, @"[`#>*_\[\]\(\)!|~-]+", " ");
        Result = Regex.Replace(Result, @"\s+", " ");
        return Result.Trim();
    }
    /// <summary>
    /// Builds a meta description.
    /// </summary>
    /// <param name="BodyText">The body text.</param>
    /// <returns>The meta description.</returns>
    static string BuildMetaDescription(string BodyText)
    {
        BodyText = Regex.Replace(BodyText ?? string.Empty, @"\s+", " ").Trim();
        return BodyText.Length <= 160 ? BodyText : BodyText.Substring(0, 160).Trim() + "...";
    }
    /// <summary>
    /// Builds meta tags.
    /// </summary>
    /// <param name="Title">The title.</param>
    /// <param name="Description">The description.</param>
    /// <param name="RelativeUrl">The relative URL.</param>
    /// <param name="Info">The build info.</param>
    /// <returns>The meta tags.</returns>
    static string BuildMetaTags(string Title, string Description, string RelativeUrl, WikiBuildInfo Info)
    {
        string BaseUrl = (Info.SiteBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        string CanonicalUrl = string.IsNullOrWhiteSpace(BaseUrl) ? RelativeUrl : BaseUrl + RelativeUrl;
        string Desc = "Author: Theo Bebekis, Title: The Corp of the World, Category: Books";
        if (!string.IsNullOrWhiteSpace(Description))
            Desc += ", Description: " + Description;

        StringBuilder Builder = new();
        Builder.Append($"<meta name=\"description\" content=\"{HtmlEscape(Desc)}\" />");
        Builder.Append($"<link rel=\"canonical\" href=\"{HtmlEscape(CanonicalUrl)}\" />");
        Builder.Append("<meta property=\"og:type\" content=\"website\" />");
        Builder.Append($"<meta property=\"og:title\" content=\"{HtmlEscape(Title)}\" />");
        Builder.Append($"<meta property=\"og:description\" content=\"{HtmlEscape(Desc)}\" />");
        Builder.Append($"<meta property=\"og:url\" content=\"{HtmlEscape(CanonicalUrl)}\" />");

        string ImageUrl = Info.DefaultSocialImageUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(ImageUrl))
        {
            if (!ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(BaseUrl))
                ImageUrl = BaseUrl + (ImageUrl.StartsWith('/') ? ImageUrl : "/" + ImageUrl);

            Builder.Append($"<meta property=\"og:image\" content=\"{HtmlEscape(ImageUrl)}\" />");
            Builder.Append("<meta name=\"twitter:card\" content=\"summary_large_image\" />");
            Builder.Append($"<meta name=\"twitter:title\" content=\"{HtmlEscape(Title)}\" />");
            Builder.Append($"<meta name=\"twitter:description\" content=\"{HtmlEscape(Desc)}\" />");
            Builder.Append($"<meta name=\"twitter:image\" content=\"{HtmlEscape(ImageUrl)}\" />");
        }

        return Builder.ToString();
    }
    /// <summary>
    /// Wraps content in the wiki layout.
    /// </summary>
    /// <param name="Title">The page title.</param>
    /// <param name="SidebarHtml">The sidebar HTML.</param>
    /// <param name="HeaderHtml">The header HTML.</param>
    /// <param name="ContentHtml">The content HTML.</param>
    /// <param name="FooterHtml">The footer HTML.</param>
    /// <param name="MetaTagsHtml">The meta tags HTML.</param>
    /// <param name="TemplateHtml">The template HTML.</param>
    /// <returns>The full page HTML.</returns>
    static string WrapInLayout(string Title, string SidebarHtml, string HeaderHtml, string ContentHtml, string FooterHtml, string MetaTagsHtml, string TemplateHtml)
    {
        return TemplateHtml
            .Replace("{{TITLE}}", HtmlEscape(Title))
            .Replace("{{HEADER}}", HeaderHtml)
            .Replace("{{SIDEBAR}}", SidebarHtml)
            .Replace("{{CONTENT}}", ContentHtml)
            .Replace("{{FOOTER}}", FooterHtml)
            .Replace("{{META_TAGS}}", MetaTagsHtml);
    }
    /// <summary>
    /// Builds header HTML.
    /// </summary>
    /// <returns>The header HTML.</returns>
    static string BuildHeaderHtml()
    {
        return """
               <header class="site-header"><div class="header-left"><button class="burger" aria-label="Menu">Menu</button><a class="home-link" href="/index.html">Go to World</a> <a class="about-link" href="/about.html">About</a></div><div class="header-right"><div class="search-box"><input id="search-input" type="search" placeholder="Search..." autocomplete="off" /><div id="search-results" class="search-results"></div></div><div class="theme-toggle" role="group" aria-label="Theme"><button class="theme-btn" data-mode="light" title="Light">Light</button><button class="theme-btn" data-mode="dark" title="Dark">Dark</button><button class="theme-btn" data-mode="auto" title="Auto">Auto</button></div><a class="buy-btn" href="https://www.amazon.com/dp/B0G2MXJ2RG" target="_blank">Buy the Book</a></div></header>
               """;
    }
    /// <summary>
    /// Builds footer HTML.
    /// </summary>
    /// <returns>The footer HTML.</returns>
    static string BuildFooterHtml()
    {
        return """
               <footer class="site-footer"><div class="footer-right"><a class="buy-footer" href="https://www.amazon.com/dp/B0G2MXJ2RG" target="_blank">Buy the Book</a></div><div><hr></div><div class="footer-left">Generated by Deltos Wiki © Theo Bebekis</div><div><p><a href="mailto:teo.bebekis@gmail.com">Official Author Email</a></p></div></footer>
               """;
    }
    /// <summary>
    /// Builds sidebar HTML.
    /// </summary>
    /// <param name="Categories">The category map.</param>
    /// <param name="Tags">The tag map.</param>
    /// <returns>The sidebar HTML.</returns>
    static string BuildSidebarHtml(Dictionary<string, List<WikiComponentInfo>> Categories, Dictionary<string, List<WikiComponentInfo>> Tags)
    {
        StringBuilder Builder = new();
        Builder.Append("<div class=\"wiki-nav\">");
        Builder.Append("<div class=\"wiki-nav-tabs\"><button class=\"wiki-nav-tab active\" data-panel=\"categories\">Categories</button><button class=\"wiki-nav-tab\" data-panel=\"tags\">Tags</button></div>");
        AppendSidebarPanel(Builder, "categories", "Filter categories...", Categories, true);
        AppendSidebarPanel(Builder, "tags", "Filter tags...", Tags, false);
        Builder.Append("</div>");
        return Builder.ToString();
    }
    /// <summary>
    /// Appends a sidebar panel.
    /// </summary>
    /// <param name="Builder">The string builder.</param>
    /// <param name="PanelName">The panel name.</param>
    /// <param name="FilterPlaceholder">The filter placeholder.</param>
    /// <param name="Groups">The grouped components.</param>
    /// <param name="Active">True if the panel is active.</param>
    static void AppendSidebarPanel(StringBuilder Builder, string PanelName, string FilterPlaceholder, Dictionary<string, List<WikiComponentInfo>> Groups, bool Active)
    {
        Builder.Append($"<section class=\"wiki-nav-panel{(Active ? " active" : "")}\" data-panel=\"{PanelName}\">");
        Builder.Append($"<input type=\"text\" class=\"quick-filter\" placeholder=\"{HtmlEscape(FilterPlaceholder)}\">");
        Builder.Append("<div class=\"wiki-nav-split\">");
        Builder.Append("<div class=\"wiki-nav-groups\">");

        bool First = true;
        foreach (string GroupName in Groups.Keys)
        {
            string SlugText = Slug(GroupName);
            Builder.Append($"<button class=\"wiki-nav-group{(First ? " active" : "")}\" data-target=\"{PanelName}-{SlugText}\">{HtmlEscape(GroupName)}</button>");
            First = false;
        }

        Builder.Append("</div>");
        Builder.Append("<div class=\"wiki-nav-items\">");

        First = true;
        foreach (KeyValuePair<string, List<WikiComponentInfo>> Pair in Groups)
        {
            string GroupSlug = Slug(Pair.Key);
            string GroupKey = PanelName + "-" + GroupSlug;
            string GroupUrl = AddNavQuery(PanelName == "categories" ? "/categories/" + GroupSlug + ".html" : "/tags/" + GroupSlug + ".html", PanelName, GroupKey, string.Empty);
            Builder.Append($"<div class=\"wiki-nav-item-list{(First ? " active" : "")}\" data-group=\"{PanelName}-{GroupSlug}\">");
            Builder.Append($"<a class=\"wiki-nav-group-link\" href=\"{GroupUrl}\">{HtmlEscape(Pair.Key)}</a>");
            Builder.Append("<ul>");
            foreach (WikiComponentInfo Component in Pair.Value)
            {
                string ItemKey = Slug(Component.Title);
                string Url = AddNavQuery("/components/" + ItemKey + ".html", PanelName, GroupKey, ItemKey);
                Builder.Append($"<li data-item=\"{ItemKey}\"><a class=\"wiki-nav-item-link\" data-item=\"{ItemKey}\" href=\"{Url}\">{HtmlEscape(Component.Title)}</a></li>");
            }
            Builder.Append("</ul></div>");
            First = false;
        }

        Builder.Append("</div></div></section>");
    }
    /// <summary>
    /// Adds navigation state query parameters to a URL.
    /// </summary>
    /// <param name="Url">The URL.</param>
    /// <param name="Tab">The selected tab.</param>
    /// <param name="Group">The selected group.</param>
    /// <param name="Item">The selected item.</param>
    /// <returns>The URL with navigation state.</returns>
    static string AddNavQuery(string Url, string Tab, string Group, string Item)
    {
        string Result = Url + "?tab=" + Uri.EscapeDataString(Tab) + "&group=" + Uri.EscapeDataString(Group);
        if (!string.IsNullOrWhiteSpace(Item))
            Result += "&item=" + Uri.EscapeDataString(Item);

        return Result;
    }
    /// <summary>
    /// Writes the search index JSON.
    /// </summary>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="Entries">The entries.</param>
    /// <param name="Result">The build result.</param>
    static void WriteSearchIndex(string OutputFolder, List<object> Entries, WikiBuildResult Result)
    {
        string Text = System.Text.Json.JsonSerializer.Serialize(Entries, Json.CreateJsonOptions(CameCase: true, Formatted: false));
        WriteText(P(OutputFolder, "search-index.json"), Text);
        AddEmitted(Result, "search-index.json");
    }
    /// <summary>
    /// Writes sitemap XML.
    /// </summary>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="SiteBaseUrl">The site base URL.</param>
    /// <param name="Urls">The URL list.</param>
    /// <param name="Result">The build result.</param>
    static void WriteSitemap(string OutputFolder, string SiteBaseUrl, List<string> Urls, WikiBuildResult Result)
    {
        string BaseUrl = (SiteBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        StringBuilder Builder = new();
        Builder.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        Builder.AppendLine("""<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">""");
        foreach (string Url in Urls.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string Loc = string.IsNullOrWhiteSpace(BaseUrl) ? Url : BaseUrl + Url;
            Builder.AppendLine($"  <url><loc>{HtmlEscape(Loc)}</loc></url>");
        }
        Builder.AppendLine("</urlset>");
        WriteText(P(OutputFolder, "sitemap.xml"), Builder.ToString());
        AddEmitted(Result, "sitemap.xml");
    }
    /// <summary>
    /// Writes robots.txt.
    /// </summary>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="SiteBaseUrl">The site base URL.</param>
    /// <param name="Result">The build result.</param>
    static void WriteRobots(string OutputFolder, string SiteBaseUrl, WikiBuildResult Result)
    {
        string BaseUrl = (SiteBaseUrl ?? string.Empty).Trim().TrimEnd('/');
        string Sitemap = string.IsNullOrWhiteSpace(BaseUrl) ? "/sitemap.xml" : BaseUrl + "/sitemap.xml";
        WriteText(P(OutputFolder, "robots.txt"), $"User-agent: *{Environment.NewLine}Allow: /{Environment.NewLine}Sitemap: {Sitemap}{Environment.NewLine}");
        AddEmitted(Result, "robots.txt");
    }
    /// <summary>
    /// Writes a page.
    /// </summary>
    /// <param name="OutputFolder">The output folder.</param>
    /// <param name="RelativePath">The relative path.</param>
    /// <param name="Html">The HTML.</param>
    /// <param name="Result">The build result.</param>
    static void WritePage(string OutputFolder, string RelativePath, string Html, WikiBuildResult Result)
    {
        WriteText(P(OutputFolder, RelativePath), Html);
        AddEmitted(Result, RelativePath);
    }

    // ● public
    /// <summary>
    /// Builds a wiki.
    /// </summary>
    /// <param name="Info">The build info.</param>
    /// <returns>The build result.</returns>
    static public WikiBuildResult Build(WikiBuildInfo Info)
    {
        WikiBuildResult Result = new();
        LogLine(Result, "Building wiki...");
        if (Info == null || Info.Project == null)
        {
            LogLine(Result, "ERROR: BuildInfo or Project is null.");
            return Result;
        }
        if (string.IsNullOrWhiteSpace(Info.OutputFolderPath))
        {
            LogLine(Result, "ERROR: Output folder path is empty.");
            return Result;
        }

        SafeCleanOutputFolder(Info.OutputFolderPath, Result);
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "components"));
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "categories"));
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "tags"));
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "assets", "css"));
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "assets", "js"));
        System.IO.Directory.CreateDirectory(P(Info.OutputFolderPath, "assets", "images"));

        List<WikiComponentInfo> Components = CollectComponents(Info);
        Dictionary<string, List<WikiComponentInfo>> Categories = BuildCategoryMap(Components);
        Dictionary<string, List<WikiComponentInfo>> Tags = BuildTagMap(Components);
        Dictionary<string, string> TermMap = BuildTermMap(Components);
        List<string> AllTitles = Components.Select(Item => Item.Title).ToList();
        string SidebarHtml = BuildSidebarHtml(Categories, Tags);
        string TemplateHtml = ReadWikiResource("wiki.html");
        List<object> SearchEntries = new();
        List<string> SitemapUrls = new();

        WriteAssets(Info.OutputFolderPath, Result);

        WikiComponentInfo Home = Components.FirstOrDefault(Item => Item.Title.IsSameText(Info.HomeComponentTitle));
        if (Home == null)
        {
            LogLine(Result, $"Home component not found: {Info.HomeComponentTitle}");
        }
        else
        {
            string MarkdownText = PreprocessMarkdown(AutoLinkTermsInMarkdown(Home.Text, TermMap), AllTitles);
            string Html = "<div class=\"buy-block\"><a href=\"https://www.amazon.com/dp/B0G2MXJ2RG\" target=\"_blank\">Buy The Corp of the World on Amazon</a></div>" + RenderMarkdownToHtml(MarkdownText);
            string BodyText = StripMarkdownToText(Home.Text);
            string Title = Home.Title;
            string MetaTags = BuildMetaTags(Title, BuildMetaDescription(BodyText), "/index.html", Info);
            string Page = WrapInLayout(Title, SidebarHtml, BuildHeaderHtml(), Html, BuildFooterHtml(), MetaTags, TemplateHtml);
            WritePage(Info.OutputFolderPath, "index.html", Page, Result);
            SearchEntries.Add(new { id = "index", title = Home.Title, aliases = Array.Empty<string>(), tags = Array.Empty<string>(), body = BodyText, url = "/index.html", category = string.Empty });
            SitemapUrls.Add("/index.html");
        }

        if (!string.IsNullOrWhiteSpace(Info.AboutComponentTitle))
        {
            WikiComponentInfo About = Components.FirstOrDefault(Item => Item.Title.IsSameText(Info.AboutComponentTitle));
            if (About != null)
            {
                string BodyText = StripMarkdownToText(About.Text);
                string Html = RenderMarkdownToHtml(About.Text);
                string MetaTags = BuildMetaTags("About the Author", BuildMetaDescription(BodyText), "/about.html", Info);
                string Page = WrapInLayout("About the Author", SidebarHtml, BuildHeaderHtml(), Html, BuildFooterHtml(), MetaTags, TemplateHtml);
                WritePage(Info.OutputFolderPath, "about.html", Page, Result);
                SearchEntries.Add(new { id = "about", title = "About the Author", aliases = Array.Empty<string>(), tags = Array.Empty<string>(), body = BodyText, url = "/about.html", category = string.Empty });
                SitemapUrls.Add("/about.html");
            }
        }

        foreach (WikiComponentInfo Component in Components.OrderBy(Item => Item.Title))
        {
            string MarkdownText = AutoLinkTermsInMarkdown(Component.Text, TermMap);
            MarkdownText = PreprocessMarkdown(MarkdownText, AllTitles);
            MarkdownText = AppendTaxonomyFooter(MarkdownText, Component.Category, Component.TagList, Categories, Tags);
            string BodyText = StripMarkdownToText(Component.Text);
            string RelativeUrl = "/components/" + Slug(Component.Title) + ".html";
            string Html = "<div class=\"buy-block\"><a href=\"https://www.amazon.com/dp/B0G2MXJ2RG\" target=\"_blank\">Buy The Corp of the World</a></div>" + RenderMarkdownToHtml(MarkdownText);
            string MetaTags = BuildMetaTags(Component.Title, BuildMetaDescription(BodyText), RelativeUrl, Info);
            string Page = WrapInLayout(Component.Title, SidebarHtml, BuildHeaderHtml(), Html, BuildFooterHtml(), MetaTags, TemplateHtml);
            WritePage(Info.OutputFolderPath, P("components", Slug(Component.Title) + ".html"), Page, Result);
            SearchEntries.Add(new { id = Slug(Component.Title), title = Component.Title, aliases = Component.AliasList, tags = Component.TagList, body = BodyText, url = RelativeUrl, category = Component.Category ?? string.Empty });
            SitemapUrls.Add(RelativeUrl);
        }

        foreach (KeyValuePair<string, List<WikiComponentInfo>> Pair in Categories)
        {
            StringBuilder Html = new();
            Html.Append($"<article><h1>{HtmlEscape(Pair.Key)}</h1><ul>");
            foreach (WikiComponentInfo Component in Pair.Value)
                Html.Append($"<li><a href=\"/components/{Slug(Component.Title)}.html\">{HtmlEscape(Component.Title)}</a></li>");
            Html.Append("</ul></article>");

            string RelativeUrl = "/categories/" + Slug(Pair.Key) + ".html";
            string PageTitle = "Category: " + Pair.Key;
            string Page = WrapInLayout(PageTitle, SidebarHtml, BuildHeaderHtml(), Html.ToString(), BuildFooterHtml(), BuildMetaTags(PageTitle, "Browse components in category " + Pair.Key + " in the world wiki.", RelativeUrl, Info), TemplateHtml);
            WritePage(Info.OutputFolderPath, P("categories", Slug(Pair.Key) + ".html"), Page, Result);
            SitemapUrls.Add(RelativeUrl);
        }

        if (Info.GenerateTagPages)
        {
            foreach (KeyValuePair<string, List<WikiComponentInfo>> Pair in Tags)
            {
                StringBuilder Html = new();
                Html.Append($"<article><h1>Tag: {HtmlEscape(Pair.Key)}</h1><ul>");
                foreach (WikiComponentInfo Component in Pair.Value)
                    Html.Append($"<li><a href=\"/components/{Slug(Component.Title)}.html\">{HtmlEscape(Component.Title)}</a></li>");
                Html.Append("</ul></article>");

                string RelativeUrl = "/tags/" + Slug(Pair.Key) + ".html";
                string PageTitle = "Tag: " + Pair.Key;
                string Page = WrapInLayout(PageTitle, SidebarHtml, BuildHeaderHtml(), Html.ToString(), BuildFooterHtml(), BuildMetaTags(PageTitle, "Browse components with tag " + Pair.Key + " in the world wiki.", RelativeUrl, Info), TemplateHtml);
                WritePage(Info.OutputFolderPath, P("tags", Slug(Pair.Key) + ".html"), Page, Result);
                SitemapUrls.Add(RelativeUrl);
            }
        }

        CopyImages(Info.Project, Info.OutputFolderPath, Result);
        WriteSitemap(Info.OutputFolderPath, Info.SiteBaseUrl, SitemapUrls, Result);
        WriteRobots(Info.OutputFolderPath, Info.SiteBaseUrl, Result);
        WriteSearchIndex(Info.OutputFolderPath, SearchEntries, Result);
        LogLine(Result, "Done.");
        return Result;
    }
}
