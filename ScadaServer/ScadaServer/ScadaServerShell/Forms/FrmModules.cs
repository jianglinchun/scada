﻿/*
 * Copyright 2018 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : Server Shell
 * Summary  : Form for editing a list of Server modules
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2018
 */

using Scada.Server.Modules;
using Scada.Server.Shell.Code;
using Scada.UI;
using System;
using System.IO;
using System.Windows.Forms;
using WinControl;

namespace Scada.Server.Shell.Forms
{
    /// <summary>
    /// Form for editing a list of Server modules.
    /// <para>Форма редактирования списка модулей Сервера.</para>
    /// </summary>
    public partial class FrmModules : Form, IChildForm
    {
        /// <summary>
        /// List item representing a module.
        /// <para>Элемент списка, представляющий модуль.</para>
        /// </summary>
        private class ModuleItem
        {
            public bool IsInitialized { get; set; }
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string Descr { get; set; }
            public ModView ModView { get; set; }

            public override string ToString()
            {
                return FileName;
            }
        }


        private readonly Settings settings; // the application settings
        private readonly ServerEnvironment environment; // the application environment


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        private FrmModules()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmModules(Settings settings, ServerEnvironment environment)
            : this()
        {
            this.settings = settings ?? throw new ArgumentNullException("settings");
            this.environment = environment ?? throw new ArgumentNullException("environment");
        }


        /// <summary>
        /// Gets or sets the object associated with the form.
        /// </summary>
        public ChildFormTag ChildFormTag { get; set; }


        /// <summary>
        /// Fills the lists of modules.
        /// </summary>
        private void FillModuleLists()
        {
            try
            {
                lbUnusedModules.BeginUpdate();
                lbActiveModules.BeginUpdate();

                // fill the list of active modules
                foreach (string fileName in settings.ModuleFileNames)
                {
                    lbActiveModules.Items.Add(new ModuleItem()
                    {
                        IsInitialized = false,
                        FileName = fileName,
                        FilePath = Path.Combine(environment.AppDirs.ModDir, fileName)
                    });
                }

                // read all available modules
                DirectoryInfo dirInfo = new DirectoryInfo(environment.AppDirs.ModDir);

                FileInfo[] fileInfoArr = dirInfo.Exists ?
                    dirInfo.GetFiles("Mod*.dll", SearchOption.TopDirectoryOnly) :
                    new FileInfo[0];

                foreach (FileInfo fileInfo in fileInfoArr)
                {
                    if (!settings.ModuleFileNames.Contains(fileInfo.Name))
                    {
                        lbUnusedModules.Items.Add(new ModuleItem()
                        {
                            IsInitialized = false,
                            FileName = fileInfo.Name,
                            FilePath = fileInfo.FullName
                        });
                    }
                }

                // select an item
                if (lbActiveModules.Items.Count > 0)
                    lbActiveModules.SelectedIndex = 0;
            }
            finally
            {
                lbUnusedModules.EndUpdate();
                lbActiveModules.EndUpdate();
            }
        }

        /// <summary>
        /// Initializes the module item if needed.
        /// </summary>
        private void InitModuleItem(ModuleItem moduleItem)
        {
            if (!moduleItem.IsInitialized)
            {
                moduleItem.IsInitialized = true;

                try
                {
                    if (environment.ModuleViews.TryGetValue(moduleItem.FileName, out ModView modView))
                    {
                        moduleItem.Descr = CorrectItemDescr(modView.Descr);
                        moduleItem.ModView = modView;
                    }
                    else if (File.Exists(moduleItem.FilePath))
                    {
                        modView = ModFactory.GetModView(moduleItem.FilePath);
                        modView.AppDirs = environment.AppDirs;

                        moduleItem.Descr = CorrectItemDescr(modView.Descr);
                        moduleItem.ModView = modView;

                        environment.ModuleViews[moduleItem.FileName] = modView;
                    }
                    else
                    {
                        moduleItem.Descr = string.Format(ServerShellPhrases.ModuleNotFound, moduleItem.FileName);
                        moduleItem.ModView = null;
                    }
                }
                catch (Exception ex)
                {
                    moduleItem.Descr = ex.Message;
                    moduleItem.ModView = null;
                }
            }
        }

        /// <summary>
        /// Correct the description of a module if needed.
        /// </summary>
        private string CorrectItemDescr(string s)
        {
            return s == null ? "" : s.Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Shows a description of the specified item.
        /// </summary>
        private void ShowItemDescr(object item)
        {
            if (item is ModuleItem moduleItem)
            {
                InitModuleItem(moduleItem);
                txtDescr.Text = moduleItem.Descr;
            }
        }

        /// <summary>
        /// Enables or disables the buttons.
        /// </summary>
        private void SetButtonsEnabled()
        {
            btnActivate.Enabled = lbUnusedModules.SelectedItem is ModuleItem;

            if (lbActiveModules.SelectedItem is ModuleItem moduleItem)
            {
                btnDeactivate.Enabled = true;
                btnMoveUp.Enabled = lbActiveModules.SelectedIndex > 0;
                btnMoveDown.Enabled = lbActiveModules.SelectedIndex < lbActiveModules.Items.Count - 1;
                btnProperties.Enabled = moduleItem.ModView != null && moduleItem.ModView.CanShowProps;
            }
            else
            {
                btnDeactivate.Enabled = false;
                btnMoveUp.Enabled = false;
                btnMoveDown.Enabled = false;
                btnProperties.Enabled = false;
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void Save()
        {
            ChildFormTag.Modified = false;
        }


        private void FrmModules_Load(object sender, EventArgs e)
        {
            Translator.TranslateForm(this, "Scada.Server.Shell.Forms.FrmModules");
            FillModuleLists();
            SetButtonsEnabled();
        }

        private void btnActivate_Click(object sender, EventArgs e)
        {
            // add the selected module to the settings
            if (lbUnusedModules.SelectedItem is ModuleItem moduleItem &&
                !settings.ModuleFileNames.Contains(moduleItem.FileName))
            {
                settings.ModuleFileNames.Add(moduleItem.FileName);
                ChildFormTag.Modified = true;

                lbUnusedModules.Items.RemoveAt(lbUnusedModules.SelectedIndex);
                lbActiveModules.SelectedIndex = lbActiveModules.Items.Add(moduleItem);
                lbActiveModules.Focus();
            }
        }

        private void btnDeactivate_Click(object sender, EventArgs e)
        {
            // remove the selected module from the settings
            if (lbActiveModules.SelectedItem is ModuleItem moduleItem &&
                settings.ModuleFileNames.Contains(moduleItem.FileName))
            {
                settings.ModuleFileNames.Remove(moduleItem.FileName);
                ChildFormTag.Modified = true;

                lbActiveModules.Items.RemoveAt(lbActiveModules.SelectedIndex);
                lbUnusedModules.SelectedIndex = lbUnusedModules.Items.Add(moduleItem);
                lbUnusedModules.Focus();
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            // move up the selected module
            if (lbActiveModules.SelectedItem is ModuleItem moduleItem)
            {
                int curInd = lbActiveModules.SelectedIndex;
                int prevInd = curInd - 1;

                if (prevInd >= 0)
                {
                    settings.ModuleFileNames.RemoveAt(curInd);
                    settings.ModuleFileNames.Insert(prevInd, moduleItem.FileName);
                    ChildFormTag.Modified = true;

                    lbActiveModules.Items.RemoveAt(curInd);
                    lbActiveModules.Items.Insert(prevInd, moduleItem);
                    lbActiveModules.SelectedIndex = prevInd;
                    lbActiveModules.Focus();
                }
            }
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            // move down the selected module
            if (lbActiveModules.SelectedItem is ModuleItem moduleItem)
            {
                int curInd = lbActiveModules.SelectedIndex;
                int nextInd = curInd + 1;

                if (nextInd < lbActiveModules.Items.Count)
                {
                    settings.ModuleFileNames.RemoveAt(curInd);
                    settings.ModuleFileNames.Insert(nextInd, moduleItem.FileName);
                    ChildFormTag.Modified = true;

                    lbActiveModules.Items.RemoveAt(curInd);
                    lbActiveModules.Items.Insert(nextInd, moduleItem);
                    lbActiveModules.SelectedIndex = nextInd;
                    lbActiveModules.Focus();
                }
            }
        }

        private void btnProperties_Click(object sender, EventArgs e)
        {
            // show properties of the selected module
            if (lbActiveModules.SelectedItem is ModuleItem moduleItem &&
                moduleItem.ModView != null && moduleItem.ModView.CanShowProps)
            {
                lbActiveModules.Focus();
                moduleItem.ModView.ShowProps();
            }
        }

        private void lbUnusedModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowItemDescr(lbUnusedModules.SelectedItem);
            SetButtonsEnabled();
        }

        private void lbActiveModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowItemDescr(lbActiveModules.SelectedItem);
            SetButtonsEnabled();
        }

        private void lbUnusedModules_DoubleClick(object sender, EventArgs e)
        {
            btnActivate_Click(null, null);
        }

        private void lbActiveModules_DoubleClick(object sender, EventArgs e)
        {
            btnProperties_Click(null, null);
        }
    }
}
