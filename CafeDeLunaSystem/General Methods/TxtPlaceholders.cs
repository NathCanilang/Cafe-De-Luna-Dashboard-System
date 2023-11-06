using System;
using System.Drawing;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class TxtPlaceholder
    {
        public class PlaceholderHandler
        {
            private readonly string placeholderText;

            public readonly Color placeholderColor = Color.Gray;
            private readonly Color originalTextColor;

            public PlaceholderHandler(string placeholderText)
            {
                this.placeholderText = placeholderText;
            }

            public void Enter(object sender, EventArgs e)
            {
                TextBox textBox = sender as TextBox;
                if (textBox.Text.Equals(this.placeholderText))
                {
                    textBox.Text = string.Empty;
                }
                textBox.ForeColor = originalTextColor;
            }

            public void Leave(object sender, EventArgs e)
            {
                TextBox textBox = sender as TextBox;

                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = this.placeholderText;
                    textBox.ForeColor = placeholderColor;
                }   
            }     
        }
        public static void SetPlaceholder(TextBox textBox, string placeholderText)
        {
            PlaceholderHandler handler = new PlaceholderHandler(placeholderText);
            textBox.Enter += handler.Enter;
            textBox.Leave += handler.Leave;
            textBox.ForeColor = handler.placeholderColor;
            textBox.Text = placeholderText;
        }
    }
}