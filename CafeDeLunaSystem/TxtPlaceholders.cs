using System;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class TxtPlaceholder
    {
        public class PlaceholderHandler
        {
            private readonly string placeholderText;

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
            }

            public void Leave(object sender, EventArgs e)
            {
                TextBox textBox = sender as TextBox;
                if (textBox.Text.Equals(""))
                {
                    textBox.Text = this.placeholderText;
                }
            }
        }
    }
}