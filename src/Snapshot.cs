using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Snapshot_tool.src
{
    public static class Snapshot
    {
        public static void CapturePhoto(int width, int height, string filePath)
        {
            //center on mouse
            int x = Cursor.Position.X - width / 2;
            int y = Cursor.Position.Y - height / 2;

            //capture pic
            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(x, y, 0, 0, bmp.Size);
                }
                bmp.Save(filePath, ImageFormat.Png); //save pic
            }
        }
    }
}