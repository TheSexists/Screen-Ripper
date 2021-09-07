using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Screen_Ripper
{
    internal abstract class Drawer
    {
        #region DLL Imports

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, CopyPixelOperation dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, CopyPixelOperation dwRop);

        [Flags()]
        public enum RedrawWindowFlags : uint
        {
            Invalidate = 0x1,
            InternalPaint = 0x2,
            Erase = 0x4,
            Validate = 0x8,
            NoInternalPaint = 0x10,
            NoErase = 0x20,
            NoChildren = 0x40,
            AllChildren = 0x80,
            UpdateNow = 0x100,
            EraseNow = 0x200,
            Frame = 0x400,
            NoFrame = 0x800
        }

        [DllImport("user32.dll")]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateSolidBrush(uint crColor);

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject([In] IntPtr hdc, [In] IntPtr hgdiobj);

        #endregion DLL Imports

        private bool running = false;
        private Thread thread;
        public Random random = new Random();
        public int screenW = Screen.PrimaryScreen.Bounds.Width;
        public int screenH = Screen.PrimaryScreen.Bounds.Height;

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(DrawLoop));
                thread.Start();
                running = true;
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread = null;
                running = false;
            }
        }

        public void DrawLoop()
        {
            while (running)
            {
                IntPtr desktop = GetDC(IntPtr.Zero);
                Draw(desktop);
                ReleaseDC(IntPtr.Zero, desktop);
            }
        }

        public void Redraw()
        {
            RedrawWindow(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, RedrawWindowFlags.AllChildren | RedrawWindowFlags.Erase | RedrawWindowFlags.Invalidate);
        }

        public abstract void Draw(IntPtr hDC);
    }

    internal class MeltColor : Drawer
    {
        private int redrawCounter;

        public override void Draw(IntPtr hDC)
        {
            int blockW = 300;
            int blockH = 300;
            int x = random.Next(0, screenW - blockW);
            int y = random.Next(0, screenH - blockH);

            BitBlt(hDC, random.Next(-100, 101), y, screenW, blockH, hDC, 0, y, CopyPixelOperation.SourceCopy);
            BitBlt(hDC, x, random.Next(-100, 101), blockW, screenH, hDC, x, 0, CopyPixelOperation.SourceCopy);

            redrawCounter++;
            if (redrawCounter >= 20)
            {
                redrawCounter = 0;
                Redraw();
                IntPtr brush = CreateSolidBrush((uint)random.Next(0, 0xffffff + 1));
                SelectObject(hDC, brush);
                PatBlt(hDC, 0, 0, screenW, screenH, CopyPixelOperation.PatInvert);
                DeleteObject(brush);
            }

            Thread.Sleep(10);
        }
    }

    internal class Waves : Drawer
    {
        private int redrawCounter;
        private Point privious = new Point(0, 0);
        private int priviousJump = 1;

        public override void Draw(IntPtr hDC)
        {
            int blockW = 45;
            int blockH = 45;

            privious = new Point(privious.X + blockH, privious.Y + blockW);
            if (privious.X >= Screen.PrimaryScreen.Bounds.Width)
                privious = new Point(0, privious.Y);

            if (privious.Y >= Screen.PrimaryScreen.Bounds.Height)
                privious = new Point(privious.X, 0);

            float time = (float)((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 31415) / 2000.0);
            Color rgb = Color.FromArgb(255,
                        (int)((Math.Sin(time) * .5f + .5f) * 255.0f),
                        (int)((Math.Sin(time + 2 * Math.PI / 3) * .5f + .5f) * 255.0f),
                        (int)((Math.Sin(time + 4 * Math.PI / 3) * .5f + .5f) * 255.0f));

            priviousJump = priviousJump > 100 ? 1 : priviousJump + 1;
            BitBlt(hDC, blockW, privious.Y, screenW, blockH + priviousJump, hDC, 1, privious.Y, CopyPixelOperation.SourceCopy);
            BitBlt(hDC, privious.X, blockH, blockW + priviousJump, screenH, hDC, privious.X, 0, CopyPixelOperation.SourceCopy);

            IntPtr brush = CreateSolidBrush((uint)ColorTranslator.ToWin32(rgb));
            SelectObject(hDC, brush);
            PatBlt(hDC, 0, 0, screenW, screenH, CopyPixelOperation.PatInvert);
            DeleteObject(brush);

            redrawCounter++;
            if (redrawCounter >= Screen.PrimaryScreen.Bounds.Width)
            {
                redrawCounter = 0;
                Redraw();
            }

            Thread.Sleep(10);
        }
    }

    internal class Test : Drawer
    {
        private int redrawCounter;
        private Point privious = new Point(0, 0);
        private int priviousJump = 1;

        public override void Draw(IntPtr hDC)
        {
            int blockW = 45;
            int blockH = 45;

            privious = new Point(privious.X + blockH, privious.Y + blockW);
            if (privious.X >= Screen.PrimaryScreen.Bounds.Width)
                privious = new Point(0, privious.Y);
            if (privious.Y >= Screen.PrimaryScreen.Bounds.Height)
                privious = new Point(privious.X, 0);

            float time = (float)((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 31415) / 2000.0);
            Color rgb = Color.FromArgb(255,
                        (int)((Math.Sin(time) * .5f + .5f) * 255.0f),
                        (int)((Math.Sin(time + 2 * Math.PI / 3) * .5f + .5f) * 255.0f),
                        (int)((Math.Sin(time + 4 * Math.PI / 3) * .5f + .5f) * 255.0f));

            priviousJump = priviousJump > 50 ? 1 : priviousJump + 5;
            //BitBlt(hDC, privious.Y, blockW, privious.X, priviousJump, hDC, priviousJump, priviousJump, CopyPixelOperation.SourceCopy);
            //BitBlt(hDC, blockW, screenH, privious.X, priviousJump, hDC, priviousJump, priviousJump, CopyPixelOperation.SourceCopy);

            Point first = new Point(screenW / 2 - 25, screenH / 2 + 25);
            IntPtr brush = CreateSolidBrush((uint)ColorTranslator.ToWin32(rgb));
            SelectObject(hDC, brush);

            int size = 10;
            PatBlt(hDC, first.X, first.Y, size, size, CopyPixelOperation.PatInvert);
            PatBlt(hDC, first.X + blockH * 2, first.Y + blockW * 2, size, size, CopyPixelOperation.PatInvert);
            PatBlt(hDC, first.X + blockH * 8, first.Y + blockW * 4, size, size, CopyPixelOperation.PatInvert);
            PatBlt(hDC, first.X + blockH * 16, first.Y + blockW * 6, size, size, CopyPixelOperation.PatInvert);
            PatBlt(hDC, first.X + blockH * 32, first.Y + blockW * 12, size, size, CopyPixelOperation.PatInvert);
            PatBlt(hDC, first.X + blockH * 64, first.Y + blockW * 24, size, size, CopyPixelOperation.PatInvert);

            //PatBlt(hDC, first.X, first.Y, size, size, CopyPixelOperation.PatInvert);
            //PatBlt(hDC, first.X - 2, first.Y + 2, size, size, CopyPixelOperation.PatInvert);
            //PatBlt(hDC, first.X - 8, first.Y + 8, size, size, CopyPixelOperation.PatInvert);
            //PatBlt(hDC, first.X - 16, first.Y + 16, size, size, CopyPixelOperation.PatInvert);
            //PatBlt(hDC, first.X - 32, first.Y + 32, size, size, CopyPixelOperation.PatInvert);
            //PatBlt(hDC, first.X - 64, first.Y + 64, size, size, CopyPixelOperation.PatInvert);

            DeleteObject(brush);

            redrawCounter++;
            if (redrawCounter >= Screen.PrimaryScreen.Bounds.Width)
            {
                redrawCounter = 0;
                Redraw();
            }

            Thread.Sleep(10);
        }
    }
}