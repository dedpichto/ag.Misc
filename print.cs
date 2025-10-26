public class PrinterPropertiesHelper
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DocumentProperties(
            IntPtr hwnd,
            IntPtr hPrinter,
            string pDeviceName,
            IntPtr pDevModeOutput,
            IntPtr pDevModeInput,
            int fMode);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        private const int DM_IN_PROMPT = 4;
        private const int DM_OUT_BUFFER = 2;

        public static void ShowPrinterPropertiesDialog(System.Windows.Window owner, string printerName)
        {
            IntPtr hPrinter = IntPtr.Zero;

            if (OpenPrinter(printerName, out hPrinter, IntPtr.Zero))
            {
                try
                {
                    IntPtr hwnd = new WindowInteropHelper(owner).Handle;

                    // DM_IN_PROMPT shows the dialog
                    // DM_OUT_BUFFER saves the settings
                    DocumentProperties(hwnd, hPrinter, printerName,
                        IntPtr.Zero, IntPtr.Zero, DM_IN_PROMPT | DM_OUT_BUFFER);
                }
                finally
                {
                    ClosePrinter(hPrinter);
                }
            }
        }
}

/ Usage:
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                string printerName = printDialog.PrintQueue.FullName;
                PrinterPropertiesHelper.ShowPrinterPropertiesDialog(this, printerName);
                // Settings are now saved for this printer!
            }
