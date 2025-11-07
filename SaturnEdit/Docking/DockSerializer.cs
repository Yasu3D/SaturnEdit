using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using SaturnEdit.Windows.Main.ChartEditor.Tabs;

namespace SaturnEdit.Docking;

public static class DockSerializer
{
    public static string Serialize()
    {
        if (MainWindow.Instance == null) return "";
        if (DockArea.Instance?.Root?.Content == null) return "";
        
        try
        {
            int indent = 0;
            string data = "";
            
            serializeWindow(MainWindow.Instance);

            if (DockArea.Instance.Root.Content is DockSplitter s)
            {
                serializeSplitter(s);
            }
            else if (DockArea.Instance.Root.Content is DockTabGroup g)
            {
                serializeGroup(g);
            }
            
            foreach (Window w in MainWindow.Instance.OwnedWindows)
            {
                if (w is not DockWindow d) continue;
                if (d.WindowContent.Content is not DockTabGroup g) continue;
                
                serializeWindow(d);
                serializeGroup(g);
            }
            
            return data;

            void serializeWindow(Window window)
            {
                string name   = window is DockWindow ? "Ext" : "Main";
                string width  = window.Bounds.Width.ToString(CultureInfo.InvariantCulture);
                string height = window.Bounds.Height.ToString(CultureInfo.InvariantCulture);
                string posX   = window.Position.X.ToString(CultureInfo.InvariantCulture);
                string posY   = window.Position.Y.ToString(CultureInfo.InvariantCulture);
                data += $"{indentString()}{name} {width} {height} {posX} {posY}\n";
            }
            
            void serializeSplitter(DockSplitter splitter)
            {
                indent++;

                string direction = splitter.Direction == GridResizeDirection.Columns ? "Col" : "Row";
                string proportion = splitter.Proportion.ToString(CultureInfo.InvariantCulture);
                data += $"{indentString()}Split {direction} {proportion}\n";

                if (splitter.ItemA.Content is DockTabGroup groupA)
                {
                    serializeGroup(groupA);
                }
                else if (splitter.ItemA.Content is DockSplitter splitterA)
                {
                    serializeSplitter(splitterA);
                }
                
                if (splitter.ItemB.Content is DockTabGroup groupB)
                {
                    serializeGroup(groupB);
                }
                else if (splitter.ItemB.Content is DockSplitter splitterB)
                {
                    serializeSplitter(splitterB);
                }
                
                indent--;
            }

            void serializeGroup(DockTabGroup group)
            {
                indent++;

                data += $"{indentString()}Group\n";

                foreach (object? obj in group.TabList.Items)
                {
                    if (obj is not DockTab tab) continue;

                    serializeTab(group, tab);
                }
                
                indent--;
            }

            void serializeTab(DockTabGroup parent, DockTab tab)
            {
                indent++;

                bool selected = Equals(parent.TabList.SelectedItem, tab);
                data += $"{indentString()}{tab.TabContent?.GetType().Name}{(selected ? " X" : "")}\n";
                
                indent--;
            }
            
            string indentString() => new(' ', indent * 4);
        }
        catch (Exception ex)
        {
            // Don't throw.
            Console.WriteLine(ex);
            return "";
        }
    }

    public static void Deserialize(string data)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (MainWindow.Instance == null) return;
            if (DockArea.Instance == null) return;

            try
            {
                DockArea.Instance.Root.Content = null;

                foreach (Window w in MainWindow.Instance.OwnedWindows)
                {
                    w.Close();
                }

                string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                Stack<DockSplitter> splitterStack = [];
                Stack<DockWindow> dockWindowStack = [];
                DockTabGroup? group = null;

                foreach (string line in lines)
                {
                    string[] split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    if (split[0] == "Main")
                    {
                        MainWindow.Instance.Width = Convert.ToDouble(split[1], CultureInfo.InvariantCulture);
                        MainWindow.Instance.Height = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);

                        int posX = Convert.ToInt32(split[3], CultureInfo.InvariantCulture);
                        int posY = Convert.ToInt32(split[4], CultureInfo.InvariantCulture);
                        MainWindow.Instance.Position = new(posX, posY);
                        continue;
                    }

                    if (split[0] == "Ext")
                    {
                        int posX = Convert.ToInt32(split[3], CultureInfo.InvariantCulture);
                        int posY = Convert.ToInt32(split[4], CultureInfo.InvariantCulture);

                        dockWindowStack.Push(new()
                        {
                            Width = Convert.ToDouble(split[1], CultureInfo.InvariantCulture),
                            Height = Convert.ToDouble(split[2], CultureInfo.InvariantCulture),
                            Position = new(posX, posY),
                        });
                        continue;
                    }

                    if (split[0] == "Split")
                    {
                        DockSplitter splitter = new()
                        {
                            Direction = split[1] == "Col" ? GridResizeDirection.Columns : GridResizeDirection.Rows,
                            Proportion = Convert.ToDouble(split[2], CultureInfo.InvariantCulture),
                        };

                        if (splitterStack.Count == 0)
                        {
                            DockArea.Instance.Root.Content ??= splitter;
                            splitterStack.Push(splitter);
                        }
                        else
                        {
                            DockSplitter parentSplitter = splitterStack.Peek();
                            if (parentSplitter.ItemA.Content == null)
                            {
                                parentSplitter.ItemA.Content = splitter;
                                splitterStack.Push(splitter);
                            }
                            else if (parentSplitter.ItemB.Content == null)
                            {
                                parentSplitter.ItemB.Content = splitter;
                                splitterStack.Pop();
                                splitterStack.Push(splitter);
                            }
                        }

                        continue;
                    }

                    if (split[0] == "Group")
                    {
                        group = new();

                        if (dockWindowStack.Count > 0)
                        {
                            dockWindowStack.Peek().WindowContent.Content = group;
                        }
                        else if (splitterStack.Count > 0)
                        {
                            DockSplitter parentSplitter = splitterStack.Peek();
                            if (parentSplitter.ItemA.Content == null)
                            {
                                parentSplitter.ItemA.Content = group;
                            }
                            else if (parentSplitter.ItemB.Content == null)
                            {
                                parentSplitter.ItemB.Content = group;
                                splitterStack.Pop();
                            }
                        }
                        else
                        {
                            DockArea.Instance.Root.Content ??= group;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ChartView3D")
                    {
                        group.TabList.Items.Add(new DockTab(new ChartView3D(), Icon.CircleShadow, "ChartEditor.ChartView3D"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ChartView2D")
                    {
                        group.TabList.Items.Add(new DockTab(new ChartView2D(), Icon.GanttChart, "ChartEditor.ChartView2D"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ChartViewTxt")
                    {
                        group.TabList.Items.Add(new DockTab(new ChartViewTxt(), Icon.TextT, "ChartEditor.ChartViewTxt"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ChartPropertiesView")
                    {
                        group.TabList.Items.Add(new DockTab(new ChartPropertiesView(), Icon.TextBulletList, "ChartEditor.ChartProperties"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ChartStatisticsView")
                    {
                        group.TabList.Items.Add(new DockTab(new ChartStatisticsView(), Icon.DataHistogram, "ChartEditor.ChartStatistics"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "ProofreaderView")
                    {
                        group.TabList.Items.Add(new DockTab(new ProofreaderView(), Icon.ApprovalsApp, "ChartEditor.Proofreader"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "EventListView")
                    {
                        group.TabList.Items.Add(new DockTab(new EventListView(), Icon.TextBulletList, "ChartEditor.EventList"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "LayerListView")
                    {
                        group.TabList.Items.Add(new DockTab(new LayerListView(), Icon.TextBulletList, "ChartEditor.LayerList"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "InspectorView")
                    {
                        group.TabList.Items.Add(new DockTab(new InspectorView(), Icon.WrenchScrewdriver, "ChartEditor.Inspector"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "CursorView")
                    {
                        group.TabList.Items.Add(new DockTab(new CursorView(), Icon.CircleHintHalfVertical, "ChartEditor.Cursor"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "AudioMixerView")
                    {
                        group.TabList.Items.Add(new DockTab(new AudioMixerView(), Icon.Speaker2, "ChartEditor.AudioMixer"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }

                        continue;
                    }

                    if (group != null && split[0] == "WaveformView")
                    {
                        group.TabList.Items.Add(new DockTab(new WaveformView(), Icon.Pulse, "ChartEditor.Waveform"));
                        if (split.Length == 2 && split[1] == "X")
                        {
                            group.TabList.SelectedIndex = group.TabList.Items.Count - 1;
                        }
                    }
                }

                foreach (DockWindow window in dockWindowStack)
                {
                    window.Show(MainWindow.Instance);
                }
            }
            catch (Exception ex)
            {
                // Don't throw.
                Console.WriteLine(ex);
                DockArea.Instance.Root = null;
            }
        });
    }
}