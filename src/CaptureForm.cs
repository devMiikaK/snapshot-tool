using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Snapshot_tool.src;

namespace Snapshot_tool
{
    public partial class CaptureForm : Form
    {
        //states
        private Hotkey _hotkeys;
        private CursorOverlay? overlay = null;
        private bool isRunning = false;
        private bool isSettingHotkey = false;
        private int indexCounter = 0;
        private Keys currentKey = Keys.S; //default key

        //window constructor
        public CaptureForm()
        {
            InitializeComponent();
            _hotkeys = new Hotkey(this.Handle);
            AppUI();

            //set default hotkey
            _hotkeys.Register(currentKey);

            this.KeyPreview = true; //let form pick up a set hotkey
            this.KeyDown += Snapshot_Keydown;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _hotkeys.Dispose(); //clean up hotkey
            if (overlay != null) overlay.Close(); //close box overlay if its open
            base.OnFormClosing(e);
        }

        //logic for hotkey
        private void Snapshot_Keydown(object? sender, KeyEventArgs e)
        {
            if (isSettingHotkey)
            {
                //available keys
                if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12 ||
                    e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z ||
                    e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                {
                    currentKey = e.KeyCode;
                }
                else
                {
                    //ignore Ctrl, alt, shift hotkeys
                    if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Menu) return;
                    currentKey = e.KeyCode;
                }

                //set keys to ui
                btnSetHotkey.Text = currentKey.ToString();
                btnSetHotkey.BackColor = Color.White;
                isSettingHotkey = false;
                _hotkeys.Register(currentKey); //register for windows
                e.SuppressKeyPress = true;
            }
        }

        //toggle logic
        private void ToggleMode()
        {
            if (!isRunning)
            {
                //start taking photos
                int w = int.Parse(txtWidth.Text);
                int h = int.Parse(txtHeight.Text);

                //pass capture function to the box overlay
                overlay = new CursorOverlay(w, h, PerformCapture, () => this.Bounds);
                overlay.Show();

                this.TopMost = true;

                btnToggle.Text = "stop";
                btnToggle.BackColor = Color.Red;
                isRunning = true;
            }
            else
            {
                //stop
                if (overlay != null)
                {
                    overlay.Close();
                    overlay = null;
                }

                this.TopMost = false;
                btnToggle.Text = "Take photos";
                btnToggle.BackColor = Color.LightGray;
                isRunning = false;
            }
        }

        private void PerformCapture()
        {
            if (overlay == null) return;
            Point originalLoc = overlay.Location;

            try
            {
                //move the overlay away for the pic
                overlay.Location = new Point(-32000, -32000);
                Application.DoEvents();
                Thread.Sleep(60);

                //filenames
                string folder = txtPath.Text;
                string prefix = txtPrefix.Text;
                string suffix = rbDate.Checked ? DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") : indexCounter++.ToString("D4"); //for date name

                //capture pic
                Snapshot.CapturePhoto(int.Parse(txtWidth.Text), int.Parse(txtHeight.Text), Path.Combine(folder, $"{prefix}{suffix}.png"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                //restore the box overlay after
                overlay.Location = originalLoc;
                overlay.Activate();
            }
        }

        private void BrowseFolder(object? sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                    indexCounter = 0; //start index from 0
                }
            }
        }

        //hotkey listener
        protected override void WndProc(ref Message m)
        {
            //nullcheck for _hotkeys
            if (_hotkeys != null && _hotkeys.IsHotkeyMessage(m))
            {
                ToggleMode();
            }
            base.WndProc(ref m);
        }
    }
}