using System;
using System.Drawing;
using System.Windows.Forms;

namespace Snapshot_tool
{
    public partial class CaptureForm : Form
    {
        //controls
        private TextBox txtWidth = null!, txtHeight = null!;
        private TextBox txtPath = null!, txtPrefix = null!;
        private RadioButton rbDate = null!, rbIndex = null!;
        private Button btnBrowse = null!;
        private Button btnToggle = null!;
        private Button btnSetHotkey = null!;

        //ui setup
        private void AppUI()
        {
            this.Text = "Snapshot-tool";
            this.Size = new Size(360, 210);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            int y = 15; //row space

            //set dimensions
            this.Controls.Add(new Label { Text = "size:", Location = new Point(15, y + 3), AutoSize = true });

            this.Controls.Add(new Label { Text = "w:", Location = new Point(60, y + 3), AutoSize = true });
            txtWidth = new TextBox { Text = "512", Location = new Point(85, y), Width = 50 };
            this.Controls.Add(txtWidth);

            this.Controls.Add(new Label { Text = "h:", Location = new Point(145, y + 3), AutoSize = true });
            txtHeight = new TextBox { Text = "512", Location = new Point(165, y), Width = 50 };
            this.Controls.Add(txtHeight);

            //set filename
            y += 35;
            this.Controls.Add(new Label { Text = "name:", Location = new Point(15, y + 3), AutoSize = true });
            txtPrefix = new TextBox { Text = "snap_", Location = new Point(60, y), Width = 80 }; //snap_ default prefix
            this.Controls.Add(txtPrefix);

            //index/date toggle
            rbIndex = new RadioButton { Text = "index", Location = new Point(150, y + 1), Checked = true, AutoSize = true };
            rbDate = new RadioButton { Text = "date", Location = new Point(210, y + 1), AutoSize = true };
            this.Controls.Add(rbIndex);
            this.Controls.Add(rbDate);

            //path selection
            y += 35;
            this.Controls.Add(new Label { Text = "save to:", Location = new Point(15, y + 3), AutoSize = true });
            string defaultPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Snapshots"); //default path
            txtPath = new TextBox
            {
                Text = defaultPath,
                Location = new Point(60, y),
                Width = 200,
                ReadOnly = false
            };
            this.Controls.Add(txtPath);

            //browse button
            btnBrowse = new Button { Text = "...", Location = new Point(260, y - 1), Width = 30, Height = 25 };
            btnBrowse.Click += BrowseFolder;
            this.Controls.Add(btnBrowse);

            //set hotkey
            y += 40;
            this.Controls.Add(new Label { Text = "hotkey:", Location = new Point(15, y + 5), AutoSize = true });
            btnSetHotkey = new Button
            {
                Text = currentKey.ToString(),
                Location = new Point(60, y),
                Width = 80,
                Height = 28,
                BackColor = Color.White
            };
            btnSetHotkey.Click += (s, e) => {
                isSettingHotkey = true;
                btnSetHotkey.Text = "?";
                btnSetHotkey.BackColor = Color.LightGray; //change color when choosing hotkey...
            };
            this.Controls.Add(btnSetHotkey);

            //take photos button. using hotkey also works
            btnToggle = new Button
            {
                Text = "take photos",
                Location = new Point(150, y),
                Width = 110,
                Height = 28,
            };
            btnToggle.Click += (s, e) => ToggleMode();
            this.Controls.Add(btnToggle);
        }
    }
}