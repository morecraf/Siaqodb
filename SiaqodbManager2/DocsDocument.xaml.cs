using AvalonDock;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using SiaqodbManager.DocSerializer;
using Sqo;
using Sqo.Documents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace SiaqodbManager
{
    /// <summary>
    /// Interaction logic for DocsDocument.xaml
    /// </summary>
    public partial class DocsDocument : DocumentContent
    {
        public DocsDocument()
        {
            InitializeComponent();
            DataContext = this;
            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            rowQueryEditor.Height = new GridLength(20);
            splitterRowQueryEditor.Visibility = Visibility.Collapsed;
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            rowQueryEditor.Height =new GridLength(200);
            splitterRowQueryEditor.Visibility = Visibility.Visible;
        }
        #region TextContent

        /// <summary>
        /// TextContent Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(DocsDocument),
                new FrameworkPropertyMetadata((string)string.Empty,
                    new PropertyChangedCallback(OnTextContentChanged)));

        /// <summary>
        /// Gets or sets the TextContent property.  This dependency property 
        /// indicates document text.
        /// </summary>
        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        /// <summary>
        /// Handles changes to the TextContent property.
        /// </summary>
        private static void OnTextContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DocsDocument)d).OnTextContentChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the TextContent property.
        /// </summary>
        protected virtual void OnTextContentChanged(DependencyPropertyChangedEventArgs e)
        {
            if (TextContentChanged != null)
                TextContentChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// event raised when text changes
        /// </summary>
        public event EventHandler TextContentChanged;
        #endregion

        public ObservableCollection<DocumentWrapper> Documents { get; set; }
        private int skip = 0;
        private int limit = 100;
        private bool allLoaded = false;
        Siaqodb siaqodb;
        string bucketName;
        int totalDocs = 0;
        public void Initialize(string bucketName, Siaqodb siaqodb)
        {
            using (StringReader s = new StringReader(syntax))
            {
                using (var reader = new XmlTextReader(s))
                {
                    textEditor1.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            queryEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            queryEditor.Text = @"
   
    Query query = new Query();
    //query.WhereEqual(""key"",""username101"")
     query.WhereStartsWith(""key"", ""username10"");

     return bucket.Find(query);

            ";
            this.siaqodb = siaqodb;
            this.bucketName = bucketName;
            SiaqodbConfigurator.SetDocumentSerializer(new MyJsonSerializer());
            Documents = new ObservableCollection<DocumentWrapper>();
            totalDocs = siaqodb.Documents[bucketName].Count();
           
            FillDocs();

        }
       
        private void listKeys_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = e.OriginalSource as ScrollViewer;
            if (sv.VerticalOffset.Equals(sv.ScrollableHeight) && !allLoaded && txtSearch.Text=="")
            {
                FillDocs();
            }
        }
        private void FillDocs()
        {
            try
            {
                IBucket bucket = siaqodb.Documents[bucketName];
                var docs = bucket.LoadAll(skip, limit);
                if (docs.Count == 0)
                    allLoaded = true;
                skip += limit;
                foreach (var d in docs)
                {
                    Documents.Add(new DocumentWrapper(d));
                }
                lblTotal.Content = Documents.Count + " documents out of " + totalDocs;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }
        private void listKeys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DocumentWrapper d = this.listKeys.SelectedItem as DocumentWrapper;
            textEditor1.Text = Newtonsoft.Json.JsonConvert.SerializeObject(d,Newtonsoft.Json.Formatting.Indented);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtSearch.Text != null && txtSearch.Text != string.Empty)
            {
                Documents.Clear();
                try
                {
                    IBucket bucket = siaqodb.Documents[bucketName];
                    var doc = bucket.Load(txtSearch.Text);

                    if (doc != null)
                    {
                        Documents.Add(new DocumentWrapper(doc));
                    }
                    lblTotal.Content = Documents.Count + " documents out of " + totalDocs;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
            }
            else
            {
                Documents.Clear();
                skip = 0;
                allLoaded = false;
                FillDocs();
            }
        }
        internal void Execute()
        {

            this.txtErrors.Text = "";
            string metBody = @" Sqo.SiaqodbConfigurator.SetLicense(@"" qU3TtvA4T4L30VSlCCGUTbooYKG1XXCnjJ+jaPPrPLaD7PdPw9HujjxmkZ467OqZ"");
  SiaqodbConfigurator.SetDocumentSerializer(new MyJsonSerializer());
using (Siaqodb siaqodb = Sqo.Internal._bs._ofm(@""" + this.siaqodb.GetDBPath() + @""",""SiaqodbManager,SiaqodbManager2""))
{
			
							IBucket bucket=siaqodb.Documents[""" + this.bucketName+@"""];
                            " + this.queryEditor.Text + @";
                           
}
							 ";
            var c = new CodeDom();
            c.AddReference(@"System.Core.dll");
            c.AddReference(@"Siaqodb.dll");
            c.AddReference(@"Newtonsoft.Json.dll");
            c.AddReference(@"SiaqodbManager.exe");
            System.CodeDom.CodeNamespace n = c.AddNamespace("LINQQuery");
           
            n.Imports("System.Collections.Generic")
            .Imports("System.Linq")
            .Imports("Sqo")
            .Imports("Sqo.Documents")
            .Imports("SiaqodbManager.DocSerializer")


            .AddClass(
              c.Class("RunQuery")
                .AddMethod(c.Method("object", "FilterByLINQ", "", metBody)));

            Assembly assembly = c.Compile(WriteErrors);
            //Assembly assembly = this.GetOntheFlyAssembly(metBody,references, namespaces);

            if (assembly != null)
            {
                Type t = assembly.GetType("LINQQuery.RunQuery");
                MethodInfo method = t.GetMethod("FilterByLINQ");

                try
                {
                    var retVal = method.Invoke(null, null);

                    IList<Document> w = ((IList<Document>)retVal);
                    Documents.Clear();
                 
                    foreach(Document d in w)
                    {
                        Documents.Add(new DocumentWrapper(d));
                    }
                    lblTotal.Content = Documents.Count + " documents out of " + totalDocs;
                    allLoaded = true;
                }
                catch (Exception ex)
                {
                    WriteErrors(ex.ToString());
                    this.tabControl1.SelectedIndex = 1;
                }
            }
            else
            {
                this.tabControl1.SelectedIndex = 1;
            }
        }
        private void WriteErrors(string errorLine)
        {
            this.txtErrors.Text += errorLine + "\r\n";
        }

        private string syntax = @"
<SyntaxDefinition name=""RdfJson"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008"">
  <Color name=""Comment"" foreground=""Green"" />
  <Color name=""Keyword"" foreground=""Red"" />
  <Color name=""String"" foreground=""Blue"" />
  <Color name=""QName"" foreground=""DarkMagenta"" />
  <Color name=""URI"" foreground=""DarkMagenta"" />
  <Color name=""Punctuation"" foreground=""DarkGreen"" />
  <Color name=""BNode"" foreground=""SteelBlue"" />
  <Color name=""LangSpec"" foreground=""DarkGreen"" />
  <Color name=""Numbers"" foreground=""Green"" />
  <Color name=""EscapedChar"" foreground=""Teal"" />
  <RuleSet>

    <Span color=""Comment"" multiline=""true"">
      <Begin>/\*</Begin>
      <End>\*/</End>
    </Span>


    <Keywords color=""Keyword"">
      <Word>null</Word>
    </Keywords>

    <Span color=""String"">
      <Begin>""</Begin>
      <End>""</End>
      <RuleSet>
        <!-- span for escape sequences -->
        <Span begin=""\\"" end="".""/>
      </RuleSet>
    </Span>

    <Rule color=""Punctuation"">
      [\[\]\{\}:,]
    </Rule>


      <!-- Keywords -->

      <Keywords color=""Keyword"" fontWeight=""bold"">
        <Word>@prefix</Word>
        <Word>@base</Word>
        <Word>a</Word>
      </Keywords>

     

      <!-- Punctuation Characters -->



      <!-- Comments -->

      <Span color=""Comment"">
        <Begin>\#</Begin>
      </Span>

      <!-- Literals -->

      <Span color=""String"" multiline=""true"" >
        <Begin>""""""</Begin>
        <End>""""""</End>
      </Span>

      <Span color=""String"" >
        <Begin>""</Begin>
        <End>""(?&lt;!\\)</End>
      </Span>


      <!-- URIs and QNames -->

      <Span color=""URI"" >
        <Begin>&lt;</Begin>
        <End>&gt;(?&lt;!\\)</End>
      </Span>

      <Rule color=""QName"">
        (\p{L}(\p{L}|\p{N}|-|_)*)?:\p{L}(\p{L}|\p{N}|-|_)*
      </Rule>

      <!-- Blank Nodes -->

      <Rule color=""BNode"">
        _:\p{L}(\p{L}|\p{N}|-|_)*
      </Rule>

      <Rule color=""BNode"">
        \[|\]|\(|\)
      </Rule>

      <!-- Language Specifiers -->

      <Rule color=""LangSpec"">
        @[A-Za-z]{2}(-[A-Za-z]+)*
      </Rule>

      <!-- Plain Literals -->

      <Keywords color=""Keyword"">
        <Word>true</Word>
        <Word>false</Word>
      </Keywords>

      <Rule color=""Numbers"">
        [\-+](\d+\.\d*[eE][\-+]?\d+|\.\d+[eE][\-+]?\d+|\d+[eE]?[\-+]\d+)
      </Rule>

      <Rule color=""Numbers"">
        [\-+]?(\d+\.\d*|\.\d+)
      </Rule>

      <Rule color=""Numbers"">
        [\-+]?\d+
      </Rule>

      <Rule color=""EscapedChar"">
        \\([trn""\\]|u[a-fA-F0-9]{4}|U[a-fA-F0-9]{8})
      </Rule>

      <Rule color=""EscapedChar"">
        \\([trn&gt;\\]|u[a-fA-F0-9]{4}|U[a-fA-F0-9]{8})
      </Rule>
    </RuleSet>
</SyntaxDefinition>";

        
    }
}
