using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace HeroesSETIDTBLEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if (File.Exists("Resources\\ObjectList.ini") & File.Exists("Resources\\StageList.ini"))
            {
                ReadObjectListData("Resources\\ObjectList.ini");
                ReadStageListData("Resources\\StageList.ini");
            }
            else
            {
                MessageBox.Show("Error loading external files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }

            ProgramIsChangingStuff = false;
        }

        public class ObjectEntry
        {
            public byte list;
            public byte type;
            public string name;
        }

        List<ObjectEntry> ObjectEntryList = new List<ObjectEntry>();

        private void ReadObjectListData(string FileName)
        {
            string[] ObjectListFile = File.ReadAllLines(FileName);

            byte List = 0;
            byte Type = 0;
            string Name = "";
            string DebugName = "";

            foreach (string i in ObjectListFile)
            {
                if (i.StartsWith("["))
                {
                    List = Convert.ToByte(i.Substring(1, 2), 16);
                    Type = Convert.ToByte(i.Substring(5, 2), 16);
                }
                else if (i.StartsWith("Object="))
                    Name = i.Split('=')[1];
                else if (i.StartsWith("Debug="))
                    DebugName = i.Split('=')[1];
                else if (i.StartsWith("EndOfFile"))
                {
                    ObjectEntryList.Add(new ObjectEntry()
                    {
                        list = List,
                        type = Type,
                        name = Name != "" ? Name : DebugName != "" ? DebugName : "Unknown/Unused"
                    });
                    break;
                }
                else if (i.Length == 0)
                {
                    ObjectEntryList.Add(new ObjectEntry()
                    {
                        list = List,
                        type = Type,
                        name = Name != "" ? Name : DebugName != "" ? DebugName : "Unknown/Unused"
                    });
                    List = 0;
                    Type = 0;
                    Name = "";
                    DebugName = "";
                }
            }
        }
        
        private void ReadStageListData(string FileName)
        {
            string[] StageListFile = File.ReadAllLines(FileName);

            foreach (string i in StageListFile)
            {
                CheckedListBox1.Items.Add(i);
            }
        }

        bool ProgramIsChangingStuff = false;

        OpenFileDialog OpenTableFile = new OpenFileDialog();

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenTableFile.Filter = ".bin files|*.bin";
            if (OpenTableFile.ShowDialog() == DialogResult.OK)
                LoadTableFile();
        }

        SaveFileDialog SaveTableFile = new SaveFileDialog();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenTableFile.FileName != null & OpenTableFile.FileName != "")
                SaveFile(OpenTableFile.FileName);
            else
            {
                SaveTableFile.Filter = ".bin files|*.bin";
                if (SaveTableFile.ShowDialog() == DialogResult.OK)
                {
                    OpenTableFile.FileName = SaveTableFile.FileName;
                    SaveFile(OpenTableFile.FileName);
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTableFile.Filter = ".bin files|*.bin";
            SaveTableFile.FileName = OpenTableFile.FileName;
            if (SaveTableFile.ShowDialog() == DialogResult.OK)
            {
                OpenTableFile.FileName = SaveTableFile.FileName;
                SaveFile(OpenTableFile.FileName);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Heroes SET ID Table Editor release 4 by igorseabra4", "About");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
                
        public class TableEntry
        {
            public ObjectEntry Object = new ObjectEntry();
            public byte[] Values;

            public TableEntry()
            {
                Values = new byte[12];
            }

            public override string ToString()
            {
                return "[" + Object.list.ToString("X2") + "] [" + Object.type.ToString("X2") + "] " + Object.name;
            }
        }

        private void LoadTableFile()
        {
            BinaryReader TableReader = new BinaryReader(new FileStream(OpenTableFile.FileName, FileMode.Open));

            ObjectNameComboBox.Items.Clear();
            byte[] FileString = TableReader.ReadBytes(20 * 512);
            TableReader.Close();
            
            for (int i = 0; i < 512; i++)
            {
                TableEntry TemporaryEntry = new TableEntry();

                TemporaryEntry.Values[0] = FileString[i * 20 + 3];
                TemporaryEntry.Values[1] = FileString[i * 20 + 2];
                TemporaryEntry.Values[2] = FileString[i * 20 + 1];
                TemporaryEntry.Values[3] = FileString[i * 20 + 0];
                TemporaryEntry.Values[4] = FileString[i * 20 + 7];
                TemporaryEntry.Values[5] = FileString[i * 20 + 6];
                TemporaryEntry.Values[6] = FileString[i * 20 + 5];
                TemporaryEntry.Values[7] = FileString[i * 20 + 4];
                TemporaryEntry.Values[8] = FileString[i * 20 + 11];
                TemporaryEntry.Values[9] = FileString[i * 20 + 10];
                TemporaryEntry.Values[10] = FileString[i * 20 + 9];
                TemporaryEntry.Values[11] = FileString[i * 20 + 8];

                TemporaryEntry.Object.list = FileString[i * 20 + 18];
                TemporaryEntry.Object.type = FileString[i * 20 + 19];

                foreach (ObjectEntry j in ObjectEntryList)
                {
                    if (j.list == TemporaryEntry.Object.list & j.type == TemporaryEntry.Object.type)
                    {
                        TemporaryEntry.Object = j;
                    }
                }

                ObjectNameComboBox.Items.Add(TemporaryEntry);
            }

            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            toolStripStatusLabel1.Text = "Loaded " + OpenTableFile.FileName;
        }

        private void SaveFile(string fileName)
        {
            BinaryWriter LayoutFileWriter = new BinaryWriter(new FileStream(fileName, FileMode.Create));

            LayoutFileWriter.BaseStream.SetLength(20 * 512);

            foreach (TableEntry i in ObjectNameComboBox.Items)
            {
                LayoutFileWriter.Write(i.Values[3]);
                LayoutFileWriter.Write(i.Values[2]);
                LayoutFileWriter.Write(i.Values[1]);
                LayoutFileWriter.Write(i.Values[0]);
                LayoutFileWriter.Write(i.Values[7]);
                LayoutFileWriter.Write(i.Values[6]);
                LayoutFileWriter.Write(i.Values[5]);
                LayoutFileWriter.Write(i.Values[4]);
                LayoutFileWriter.Write(i.Values[11]);
                LayoutFileWriter.Write(i.Values[10]);
                LayoutFileWriter.Write(i.Values[9]);
                LayoutFileWriter.Write(i.Values[8]);
                LayoutFileWriter.Write(new byte[6]);
                LayoutFileWriter.Write(i.Object.list);
                LayoutFileWriter.Write(i.Object.type);
            }

            LayoutFileWriter.Close();
            toolStripStatusLabel1.Text = "Loaded " + OpenTableFile.FileName;
        }

        byte[] Sel = { 1, 2, 4, 8, 16, 32, 64, 128 };

        private void ObjectNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProgramIsChangingStuff = true;

            for (int j = 0; j < 12; j++)
                for (int i = 0; i < 8; i++)
                    if (((ObjectNameComboBox.SelectedItem as TableEntry).Values[j] & Sel[i]) == Sel[i])
                        CheckedListBox1.SetItemCheckState(j * 8 + i, CheckState.Checked);
                    else
                        CheckedListBox1.SetItemCheckState(j * 8 + i, CheckState.Unchecked);

            ProgramIsChangingStuff = false;
        }
        
        private void CheckedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!ProgramIsChangingStuff & ObjectNameComboBox.SelectedIndex > -1)
            {
                if (e.NewValue == CheckState.Checked)
                    (ObjectNameComboBox.SelectedItem as TableEntry).Values[e.Index / 8] = (byte)((ObjectNameComboBox.SelectedItem as TableEntry).Values[e.Index / 8] | Sel[e.Index % 8]);
                else if (e.NewValue == CheckState.Unchecked)
                    (ObjectNameComboBox.SelectedItem as TableEntry).Values[e.Index / 8] = (byte)((ObjectNameComboBox.SelectedItem as TableEntry).Values[e.Index / 8] ^ Sel[e.Index % 8]);
            }
        }
    }
}