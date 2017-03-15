﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Curvature
{
    public partial class EditWidgetBehaviorSet : UserControl, IInputBroker
    {
        private Project EditProject;
        private HashSet<Behavior> EditBehaviors;

        public EditWidgetBehaviorSet(Project project, string setname, HashSet<Behavior> behaviors)
        {
            InitializeComponent();
            EditProject = project;
            EditBehaviors = behaviors;

            EnabledBehaviorsListBox.ItemCheck += (e, args) =>
            {
                RefreshTimer.Enabled = true;
            };

            BehaviorSetNameLabel.Text = $"Behavior Set: {setname}";

            foreach (Behavior b in project.Behaviors)
            {
                EnabledBehaviorsListBox.Items.Add(b, EditBehaviors.Contains(b));
            }

            RefreshInputControls();

            BehaviorScoresListView.MouseDoubleClick += (e, args) =>
            {
                var item = BehaviorScoresListView.GetItemAt(args.Location.X, args.Location.Y);
                if (item == null)
                    return;

                EditProject.NavigateTo(item.Tag as Behavior);
            };
        }

        public double GetInputValue(InputAxis axis)
        {
            foreach (EditWidgetConsiderationInput input in InputFlowPanel.Controls)
            {
                if (input.Tag == axis)
                    return input.GetNormalizedValue();
            }

            return 0.0;
        }

        public void RefreshInputs()
        {
            double winscore = 0.0;
            Behavior winbehavior = null;

            BehaviorScoresListView.Items.Clear();
            foreach (Behavior b in EnabledBehaviorsListBox.CheckedItems)
            {
                double score = b.Score(this);
                if (score > winscore)
                {
                    winscore = score;
                    winbehavior = b;
                }

                var item = new ListViewItem(new string[] { b.ReadableName, $"{b.Weight:f3}", $"{score:f3}" });
                item.Tag = b;
                BehaviorScoresListView.Items.Add(item);
            }

            if (winbehavior != null)
            {
                WinningBehaviorLabel.Text = $"Winner: {winbehavior.ReadableName} ({winscore:f3})";
            }
            else
            {
                WinningBehaviorLabel.Text = "";
            }
        }

        private void RefreshInputControls()
        {
            foreach (Control ctl in InputFlowPanel.Controls)
                ctl.Dispose();

            InputFlowPanel.Controls.Clear();


            var inputs = new Dictionary<string, InputAxis>();

            foreach (Behavior b in EnabledBehaviorsListBox.CheckedItems)
            {
                foreach (Consideration c in b.Considerations)
                {
                    if (inputs.ContainsKey(c.Input.KBRecord.ReadableName))
                        inputs.Add(c.Input.KBRecord.ReadableName, c.Input.Union(inputs[c.Input.KBRecord.ReadableName]));
                    else
                        inputs.Add(c.Input.KBRecord.ReadableName, c.Input);
                }
            }

            foreach (var rec in inputs)
            {
                var ctl = new EditWidgetConsiderationInput(rec.Value, this);
                ctl.Tag = rec.Value;
                InputFlowPanel.Controls.Add(ctl);
            }

            RefreshInputs();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshInputControls();
            RefreshTimer.Enabled = false;
        }
    }
}
