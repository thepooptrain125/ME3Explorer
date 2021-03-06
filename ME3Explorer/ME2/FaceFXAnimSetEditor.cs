﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ME3Explorer.Unreal;
using Be.Windows.Forms;
using ME3Explorer;
using ME3Explorer.Packages;
using ME3Explorer.FaceFX;
using ME3Explorer.SharedUI;

namespace ME2Explorer
{
    public partial class FaceFXAnimSetEditor : WinFormsBase
    {
        public List<int> Objects;
        public ME2FaceFXAnimSet FaceFX;

        public FaceFXAnimSetEditor()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog {Filter = "*.pcc|*.pcc"};
            if (d.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LoadME2Package(d.FileName);
                    Objects = new List<int>();
                    IReadOnlyList<ExportEntry> Exports = Pcc.Exports;
                    for (int i = 0; i < Exports.Count; i++)
                    {
                        if (Exports[i].ClassName == "FaceFXAnimSet")
                            Objects.Add(i);
                    }

                    ListRefresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\n" + ex.Message);
                }
            }
        }

        public void ListRefresh()
        {
            listBox1.Items.Clear();
            foreach(int n in Objects)
                listBox1.Items.Add($"#{n} : {Pcc.Exports[n].FullPath}");
        }

        private void FaceFXRefresh(int n, IEnumerable<string> expandedNodes = null, string topNodeName = null)
        {
            if (FaceFX == null)
                return;
            hb1.ByteProvider = new DynamicByteProvider(Pcc.Exports[Objects[n]].Data);
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(FaceFX.HeaderToTree());
            nameAllNodes(treeView1.Nodes);
            treeView2.Nodes.Clear();
            treeView2.Nodes.Add(FaceFX.DataToTree());
            nameAllNodes(treeView2.Nodes);
            TreeNode[] nodes;
            if (expandedNodes != null)
            {
                foreach (string item in expandedNodes)
                {
                    nodes = treeView2.Nodes.Find(item, true);
                    if (nodes.Length > 0)
                    {
                        foreach (var node in nodes)
                        {
                            node.Expand();
                        }
                    }
                }
            }
            nodes = treeView2.Nodes.Find(topNodeName, true);
            if (nodes.Length > 0)
            {
                treeView2.TopNode = nodes[0];
            }
        }

        private static void nameAllNodes(TreeNodeCollection Nodes)
        {
            List<TreeNode> allNodes = Nodes.Cast<TreeNode>().ToList();
            //flatten tree of nodes into list.
            for (int i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].Name = i.ToString();
                allNodes.AddRange(allNodes[i].Nodes.Cast<TreeNode>());
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            FaceFX = new ME2FaceFXAnimSet(Pcc, Pcc.Exports[Objects[n]]);
            FaceFXRefresh(n);
        }

        private void recreateAndDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            SaveFileDialog d = new SaveFileDialog
            {
                FileName = Pcc.Exports[Objects[n]].ObjectName + ".fxa",
                Filter = "*.fxa|*.fxa"
            };
            if(d.ShowDialog() == DialogResult.OK)
            {
                FaceFX.DumpToFile(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Pcc == null)
                return;
            Pcc.Save();
            MessageBox.Show("Done.");
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FaceFX == null)
                return;
            FaceFX.Save();
            MessageBox.Show("Done.");
        }

        private void treeView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //TreeNode t = treeView2.SelectedNode;
            //if (t == null || t.Parent == null)
            //    return;
            //TreeNode t1 = t.Parent;
            //if (t1 == null || t1.Parent == null)
            //    return;
            //TreeNode t2 = t1.Parent;
            //if (t2 == null || t2.Parent == null)
            //    return;
            //string result; int i; float f = 0;
            //if (t2.Text == "Entries")
            //{
            //    int entidx = t1.Index;
            //    int subidx = t.Index;
            //    FaceFXAnimSet.FaceFXLine d = FaceFX.Data.Data[entidx];
            //    switch (subidx)
            //    {
            //        case 0://unk1
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.Name.ToString(), true);
            //            i = -1;
            //            if (int.TryParse(result, out i) && i >= 0 && i < FaceFX.Header.Names.Length)
            //                d.Name = i;
            //            break;
            //        case 4://FadeInTime
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.FadeInTime.ToString(), true);
            //            if (float.TryParse(result, out f))
            //                d.FadeInTime = f;
            //            break;
            //        case 5://FadeInTime
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.FadeOutTime.ToString(), true);
            //            if (float.TryParse(result, out f))
            //                d.FadeOutTime = f;
            //            break;
            //        case 6://unk2
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.unk2.ToString(), true);
            //            i = -1;
            //            if (int.TryParse(result, out i) && i >= 0 && i < FaceFX.Header.Names.Length)
            //                d.unk2 = i;
            //            break;
            //        case 7://Path
            //            d.path = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.path, true);
            //            break;
            //        case 8://ID
            //            d.ID = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.ID, true);
            //            break;
            //        case 9://unk3
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.unk3.ToString(), true);
            //            i = -1;
            //            if (int.TryParse(result, out i) && i >= 0 && i < FaceFX.Header.Names.Length)
            //                d.unk3 = i;
            //            break;
            //        default:
            //            return;
            //    }
            //    FaceFX.Data.Data[entidx] = d;
            //}
            //else if(t2.Parent.Text == "Entries")
            //{
            //    int entidx = t2.Index;
            //    int subidx = t1.Index;
            //    int subsubidx = t.Index;
            //    FaceFXAnimSet.FaceFXLine d = FaceFX.Data.Data[entidx];
            //    switch (subidx)
            //    {
            //        case 1:
            //            FaceFXAnimSet.NameRef u = d.animations[subsubidx];
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", u.index + " ; " + u.unk2, true);
            //            string[] reslist = result.Split(';');
            //            if (reslist.Length != 2)
            //                return;
            //            if (int.TryParse(reslist[0].Trim(), out i))
            //                u.index = i;
            //            else
            //                return;
            //            if (int.TryParse(reslist[1].Trim(), out i))
            //                u.unk2 = i;
            //            else
            //                return;
            //            d.animations[subsubidx] = u;
            //            break;
            //        case 2:
            //            FaceFXAnimSet.ControlPoint u2 = d.points[subsubidx];
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", u2.time + " ; " + u2.weight + " ; " + u2.inTangent + " ; " + u2.leaveTangent, true);
            //            reslist = result.Split(';');
            //            if (reslist.Length != 4)
            //                return;
            //            if (float.TryParse(reslist[0].Trim(), out f))
            //                u2.time = f;
            //            else
            //                return;
            //            if (float.TryParse(reslist[1].Trim(), out f))
            //                u2.weight = f;
            //            else
            //                return;
            //            if (float.TryParse(reslist[2].Trim(), out f))
            //                u2.inTangent = f;
            //            else
            //                return;
            //            if (float.TryParse(reslist[3].Trim(), out f))
            //                u2.leaveTangent = f;
            //            else
            //                return;
            //            d.points[subsubidx] = u2;
            //            break;
            //        case 3:
            //            result = PromptDialog.Prompt(null, "Please enter new value", "ME3Explorer", d.numKeys[subsubidx].ToString(), true);
            //            if (int.TryParse(result.Trim(), out i))
            //                d.numKeys[subsubidx] = i;
            //            else
            //                return;
            //            break;
            //    }
            //    FaceFX.Data.Data[entidx] = d;
            //}
            //int n = listBox1.SelectedIndex;
            //if (n == -1)
            //    return;
            //List<TreeNode> allNodes = treeView2.Nodes.Cast<TreeNode>().ToList();
            ////flatten tree of nodes into list.
            //for (int j = 0; j < allNodes.Count(); j++)
            //{
            //    allNodes.AddRange(allNodes[j].Nodes.Cast<TreeNode>());
            //}
            //var expandedNodes = allNodes.Where(x => x.IsExpanded).Select(x => x.Name);
            //FaceFXRefresh(n, expandedNodes, treeView2.TopNode.Name);
        }

        private void cloneEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView2.SelectedNode;
            TreeNode t1 = t?.Parent;
            if (t1 == null || t1.Text != "Entries" || FaceFX == null)
                return;
            FaceFX.CloneEntry(t.Index);
            FaceFX.Save();
            int n = listBox1.SelectedIndex;
            listBox1.SelectedIndex = -1;
            listBox1.SelectedIndex = n;
        }

        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView2.SelectedNode;
            TreeNode t1 = t?.Parent;
            if (t1 == null || t1.Text != "Entries" || FaceFX == null)
                return;
            FaceFX.RemoveEntry(t.Index);
            FaceFX.Save();
            int n = listBox1.SelectedIndex;
            listBox1.SelectedIndex = -1;
            listBox1.SelectedIndex = n;
        }

        private void moveEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView2.SelectedNode;
            TreeNode t1 = t?.Parent;
            if (t1 == null || t1.Text != "Entries" || FaceFX == null)
                return;
            string result = PromptDialog.Prompt(null, "Please enter new index", "ME3Explorer", t.Index.ToString(), true);
            if (int.TryParse(result, out int i))
            {
                FaceFX.MoveEntry(t.Index, i);
                FaceFX.Save();
                int n = listBox1.SelectedIndex;
                listBox1.SelectedIndex = -1;
                listBox1.SelectedIndex = n;
            }
        }

        private void addNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FaceFX == null)
                return;
            string result = PromptDialog.Prompt(null, "Please enter new name to add", "ME3Explorer", "", true);
            if (result != "")
            {
                FaceFX.AddName(result);
                FaceFX.Save();
                int n = listBox1.SelectedIndex;
                listBox1.SelectedIndex = -1;
                listBox1.SelectedIndex = n;
            }
        }
    }
}
