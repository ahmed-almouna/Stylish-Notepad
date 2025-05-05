using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace Notepad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string defaultFileName = "Untitled.txt";

        bool fileInSync = true;
        string filePath = null;
        string fileName = null;
        bool isMenuExitClicked = false; //if menu exit is executed don't call 'Closing' event

        public MainWindow()
        {
            InitializeComponent();
            Clipboard.Clear();
        }


        // --------------------------------------------------------------------------- FILE MENU BOTTUN ----------------------------------------------------------------------------
        /*
         *  Method      : mnuSave_Click()
         *  Description :
         *    Save the contents of the edit pad into the currently open file or a create a new file otherwise.
         *  Parameters  :
         *    object sender     = XAML
         *    RoutedEventArgs e = XAML
         *  Returns     :
         *    None.
         */
        private void mnuSave_Click(object sender, RoutedEventArgs e) 
        {
            if (filePath != null)
            {
                File.WriteAllText(filePath, txtInput.Text);
                fileInSync = true;
                //this.Title = fileName;
            }
            else
            {
                saveFile(defaultFileName);
            }
        }

        /*
        *  Method      : mnuSaveAs_Click()
        *  Description :
        *    Save the contents of the edit pad into a new file.
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void mnuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            saveFile(defaultFileName);
        }

        /*
        *  Method      : saveFile()
        *  Description :
        *    Save the contents of the edit pad into a new file.
        *  Parameters  :
        *    string newFileName = the default name to be put in 'File Name' section.
        *  Returns     :
        *    bool operationCompleted = true if save operation was completed, false otherwise.
        */
        private bool saveFile(string newFileName)
        {
            /* initialize an sfd */
            bool operationCompleted = false;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text documents | *.txt";
            sfd.FileName = newFileName;

            /* save contents to file */
            if (sfd.ShowDialog() == true)
            {
                operationCompleted = true;
                filePath = sfd.FileName;
                fileName = System.IO.Path.GetFileName(filePath);
                this.Title = fileName;
                File.WriteAllText(filePath, txtInput.Text);
                fileInSync = true;             
            }

            return operationCompleted;
        }

        /*
        *  Method      : txtInput_TextChanged()
        *  Description :
        *    Keep track of changes to edit pad & of the number of characters currently in it. *NOTE: newline is 2 chars
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            fileInSync = false;
            int characterCount = txtInput.Text.Length;
            characterCounter.Header = $"Character count: {characterCount}"; 
        }

        /*
        *  Method      : mnuNew_Click()
        *  Description :
        *    Create a new edit pad in a new window.
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void mnuNew_Click(object sender, RoutedEventArgs e)
        {
            isMenuExitClicked = true;
            Window newFile = new MainWindow();
            bool result = closeFile(false);

            if (result == true) //show new window
            {
                newFile.Show();
            }
        }

        /*
        *  Method      : mnuOpen_Click()
        *  Description :
        *    Open a file in edit pad.
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void mnuOpen_Click(object sender, RoutedEventArgs e)
        {
            /* initialize an ofd */
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text documents | *.txt";
            bool result = closeFile(true);

            /* open file only when changed are dealt with & a file is chosen to be opened */
            if (result == true) //changes saved or ignored
            {
                bool? openResult = ofd.ShowDialog();
                if (openResult == true) //a file was chosen
                {
                    /* initialize edit pad with file */
                    filePath = ofd.FileName;
                    fileName = System.IO.Path.GetFileName(filePath); ;
                    this.Title = fileName;
                    string content = File.ReadAllText(ofd.FileName);
                    txtInput.Text = content;
                    fileInSync = true;
                }
            }
        }

        /*
        *  Method      : mnuExit_Click()
        *  Description :
        *    Exit edit pad.
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            isMenuExitClicked = true;
            closeFile(false);
        }

        /*
        *  Method      : X_Click()
        *  Description :
        *    Exit edit pad (using X icon).
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void X_Click(object sender, CancelEventArgs e)
        {
            if (!isMenuExitClicked)
            {
                bool result = closeFile(true);
                if (!result) { e.Cancel = true; }
            }
            isMenuExitClicked = false;
        }

        /*
        *  Method      : closeFile()
        *  Description :
        *    Handle closing an edit pad.
        *  Parameters  :
        *    bool keepWindow = keep the current window open?
        *  Returns     :
        *    Bool operationCompleted = true if close operation was completed, false otherwise. *NOTE: window is not necessary closed in a complete operation
        */
        private bool closeFile(bool keepWindow)
        {
            bool operationCompleted = false;
            if (fileInSync)
            {
                operationCompleted = true;
                if (!keepWindow) { this.Close(); }
            }
            else //handle un-saved changes in edit pad
            {
                MessageBoxResult mbResult = MessageBox.Show("Would you like to save changes?", "Save file", MessageBoxButton.YesNoCancel);

                /* handle user choice */
                if (mbResult == MessageBoxResult.Yes)
                {
                    if (filePath != null) 
                    {
                        operationCompleted = saveFile(filePath); //file last saved (to)
                    }
                    else
                    {
                        operationCompleted = saveFile(defaultFileName);
                    }

                    if (operationCompleted)
                    {
                        if (!keepWindow) { this.Close(); }
                    }
                }
                else if (mbResult == MessageBoxResult.No)
                {
                    operationCompleted = true;
                    if (!keepWindow) { this.Close(); }
                }  
            }

            return operationCompleted;
        }

        /*
        *  Method      : mnuAbout_Click()
        *  Description :
        *    Show about box.
        *  Parameters  :
        *    object sender     = XAML
        *    RoutedEventArgs e = XAML
        *  Returns     :
        *    None.
        */
        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }



    }
}
