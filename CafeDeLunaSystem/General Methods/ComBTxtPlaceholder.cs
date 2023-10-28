using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class ComBTxtPlaceholder
    {
        public class PlaceholderHandler
        {
            private readonly string placeholderComBText;
            public PlaceholderHandler(string placeholderText)
            {
                this.placeholderComBText = placeholderText;
            }
            public void Enter(object sender, EventArgs e)
            {
                ComboBox comBox = sender as ComboBox;
                if (comBox.Text.Equals(this.placeholderComBText))
                {
                    comBox.Text = string.Empty;
                }
            }

            public void Leave(object sender, EventArgs e)
            {
                ComboBox comBox = sender as ComboBox;
                if (comBox.Text.Equals(""))
                {
                    comBox.Text = this.placeholderComBText;
                }
            }
        }
        public static void SetPlaceholder(ComboBox comBox, string placeholderText)
        {
            PlaceholderHandler handler = new PlaceholderHandler(placeholderText);
            comBox.Enter += handler.Enter;
            comBox.Leave += handler.Leave;
        }
    }
}
