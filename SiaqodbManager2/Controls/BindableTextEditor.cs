
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace SiaqodbManager.Controls
{
    public class BindableTextEditor:TextEditor
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(BindableTextEditor), new FrameworkPropertyMetadata(null, OnDocumentChanged));
        public BindableTextEditor()
        {
            SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        }

        public new string Text
        {
            get
            {
                return ((TextDocument)GetValue(DocumentProperty)).Text;
            }

            set
            {
                var document = new TextDocument {Text = value};
                SetValue(DocumentProperty, document);
  
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            SetCurrentValue(TextProperty, base.Document.Text);
            base.OnLostKeyboardFocus(e);
        }

        public static void OnDocumentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var textEditor = (TextEditor)obj;

            if (!String.Equals(textEditor.Text, (string) args.NewValue))
            {
                textEditor.Text = (string) args.NewValue;
            }
        }
    }
}
