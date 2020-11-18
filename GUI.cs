﻿using System;
using Autodesk.Windows;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System.Windows.Input;

namespace AutoCadGcode
{
    public class GUI
    {
        public delegate void VirtaulParametersChangedHandler();
        public static event VirtaulParametersChangedHandler VirtaulParametersChangedEvent;

        //Need to reciving information about selected object
        public static UserEntity ActiveEntity { get; set; } = null;
        //Store last selected object`s properties for display they.
        //Diiference with activeEntity is that after removing the selection
        //information is not lost here
        private static Properties _virtualProperies = new Properties();
        public static Properties VirtualProperies
        {
            get { return _virtualProperies; }
            set
            {
                _virtualProperies = value;
                GUI.VirtaulParametersChangedEvent?.Invoke();
            }
        }
        
        protected RibbonButton setPumpingTrueButton = new RibbonButton();
        protected RibbonButton setPumpingFalseButton = new RibbonButton();
        protected RibbonSpinner setOrderSpinner = new RibbonSpinner();
        protected RibbonButton setOrderButton = new RibbonButton();
        protected RibbonPanelSource rbPrintablePanelSource = new RibbonPanelSource();
        protected RibbonPanel rbPrintablePanel = new RibbonPanel();

        protected RibbonButton setFirstButton = new RibbonButton();
        protected RibbonButton setLastButton = new RibbonButton();
        private RibbonPanelSource rbNotPrintablePanelSource = new RibbonPanelSource();
        private RibbonPanel rbNotPrintablePanel = new RibbonPanel();

        protected RibbonButton validateEntitiesButton = new RibbonButton();
        protected RibbonButton buildGcodeButton = new RibbonButton();
        private RibbonPanelSource rbValidatePanelSource = new RibbonPanelSource();
        private RibbonPanel rbValidatePanel = new RibbonPanel();

        private RibbonTab rbTab = new RibbonTab();
        private RibbonControl rbCntrl = ComponentManager.Ribbon;

        public GUI()
        {
            CreateGUI();
            CreateHandling();
        }        

        private void CreateGUI()
        {
            /**
             * Properties
             */
            setPumpingTrueButton.Id = "_setPumpingTrueButton";
            setPumpingTrueButton.CommandHandler = new SetPumpingTrueHandler();
            setPumpingTrueButton.Size = RibbonItemSize.Standard;
            setPumpingTrueButton.Text = "С бетоном";
            setPumpingTrueButton.ShowText = true;

            setPumpingFalseButton.Id = "_setPumpingFalseButton";
            setPumpingFalseButton.CommandHandler = new SetPumpingFalseHandler();
            setPumpingFalseButton.Size = RibbonItemSize.Standard;
            setPumpingFalseButton.Text = "Без бетона";
            setPumpingFalseButton.ShowText = true;

            setOrderSpinner.Id = "_setOrder";
            setOrderSpinner.Size = RibbonItemSize.Standard;
            setOrderSpinner.Text = "Порядок печати";
            setOrderSpinner.ShowText = true;
            setOrderSpinner.ValueChanged += SetOrderHandler;
            setOrderSpinner.Value = 0;
            setOrderSpinner.Minimum = -1;
            setOrderSpinner.Maximum = int.MaxValue;
            setOrderSpinner.Width = 150;
            setOrderSpinner.IsEditable = true;
            setOrderSpinner.IsEnabled = true;

            setOrderButton.Id = "_setOrderButton";
            setOrderButton.CommandHandler = new SetOrderHandler();
            setOrderButton.Size = RibbonItemSize.Standard;
            setOrderButton.Text = "Применить";
            setOrderButton.ShowText = true;

            rbPrintablePanelSource.Title = "Параметры печатных линий";
            rbPrintablePanelSource.Items.Add(setPumpingTrueButton);
            rbPrintablePanelSource.Items.Add(new RibbonSeparator());
            rbPrintablePanelSource.Items.Add(setPumpingFalseButton);
            rbPrintablePanelSource.Items.Add(new RibbonRowBreak());
            rbPrintablePanelSource.Items.Add(setOrderSpinner);
            rbPrintablePanelSource.Items.Add(setOrderButton);

            rbPrintablePanel.Source = rbPrintablePanelSource;

            /**
             * Not printable parameters
             */

            setFirstButton.Id = "_setFirst";
            setFirstButton.CommandHandler = new SetFirstHandler();
            setFirstButton.Size = RibbonItemSize.Standard;
            setFirstButton.Text = "Первая";
            setFirstButton.ShowText = true;

            setLastButton.Id = "_setLast";
            setLastButton.CommandHandler = new SetLastHandler();
            setLastButton.Size = RibbonItemSize.Standard;
            setLastButton.Text = "Последняя";
            setLastButton.ShowText = true;

            rbNotPrintablePanelSource.Title = "Параметры непечатных линий";
            rbNotPrintablePanelSource.Items.Add(setFirstButton);
            rbNotPrintablePanelSource.Items.Add(new RibbonSeparator());
            rbNotPrintablePanelSource.Items.Add(setLastButton);
            rbNotPrintablePanel.Source = rbNotPrintablePanelSource;

            /**
             * Validation and building
             */

            validateEntitiesButton.Id = "_validateEntitiesButton";
            validateEntitiesButton.CommandHandler = new ValidateEntitiesHandler();
            validateEntitiesButton.Size = RibbonItemSize.Standard;
            validateEntitiesButton.Text = "Валидация";
            validateEntitiesButton.ShowText = true;

            buildGcodeButton.Id = "_buildGcodeButton";
            buildGcodeButton.CommandHandler = new buildGcodeHandler();
            buildGcodeButton.Size = RibbonItemSize.Standard;
            buildGcodeButton.Text = "Построить Gcode";
            buildGcodeButton.ShowText = true;

            rbValidatePanelSource.Title = "Валидация и запуск";
            rbValidatePanelSource.Items.Add(validateEntitiesButton);
            rbValidatePanelSource.Items.Add(new RibbonSeparator());
            rbValidatePanelSource.Items.Add(buildGcodeButton);
            rbValidatePanel.Source = rbValidatePanelSource;

            rbTab.Title = "Trace Maker";
            rbTab.Id = "_TraceMakerTab";

            rbTab.Panels.Add(rbPrintablePanel);
            rbTab.Panels.Add(rbNotPrintablePanel);
            rbTab.Panels.Add(rbValidatePanel);


            rbCntrl.Tabs.Add(rbTab);

            rbTab.IsActive = true;
        }
        private void CreateHandling()
        {
            Global.doc.ImpliedSelectionChanged += OnChangeSelectedObjectHandler;
            //Global.editor.SelectionAdded += OnChangeSelectedObjectHandler;
            //Global.editor.SelectionRemoved += OnChangeSelectedObjectHandler;

            XDataManage.PropertiesChangeEvent += OnChangeParametersHandler;
            Validation.ValidateEntitiesEvent += ValidationChecking;
            GUI.VirtaulParametersChangedEvent += OnVirtualParametersChangedHandler;
        }

        //Function for changing ribbon items attributes after incoming properties recived
        private void AttributesChanging()
        {
            //Set attributes that are not affected by deselecting
            this.setOrderSpinner.Value = VirtualProperies.Order;

            if (ActiveEntity != null) //if we have selected entity then set some attributes
            {
                //TODO
            }
            else //If we have not selected entity then clear some attributes
            {
                //TODO
            }
        }
        
        //Then validation changing we need to disable or enable the Build button
        private void ValidationChecking(bool isValidated)
        {
            if (isValidated == true)
                this.buildGcodeButton.IsEnabled = true;
            else
                this.buildGcodeButton.IsEnabled = false;
        }
        //Handler for reciving information about selected entity
        private void OnChangeSelectedObjectHandler(object sender, EventArgs e)
        {

            PromptSelectionResult res = Global.editor.SelectImplied();
            if (res.Value != null)
            {
                var list = API.ListFromSelecion(res);
                if (list != null && list.Count > 0)
                    ActiveEntity = API.GetXData(list).Last<UserEntity>();
            }
            else
                ActiveEntity = null;

            if (ActiveEntity != null)
            {
                VirtualProperies = ActiveEntity.properties;
            }
        }
        
        //Handler for situation then entities lost focus after changing parameter
        private void OnChangeParametersHandler(UserEntity uEntity)
        {
            if (ActiveEntity == null) //Check for selected entity
                VirtualProperies = uEntity.properties;
        }
        //Handler for 
        private void OnVirtualParametersChangedHandler()
        {
            AttributesChanging();
        }

        //Handler for transfer value from orderSpinner to virtualProperies
        private void SetOrderHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            VirtualProperies.Order = Convert.ToInt32((sender as RibbonSpinner).TextValue);
        }
    }

    public class SetPumpingTrueHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetPumpingTrue();
        }
    }

    public class SetPumpingFalseHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetPumpingFalse();
        }
    }

    public class SetOrderHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            //TODO: need think about avoid using Global field
            API.SetOrder(GUI.VirtualProperies);
        }
    }

    public class SetFirstHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetFirst();
        }
    }

    public class SetLastHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.SetLast();
        }
    }

    public class ValidateEntitiesHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.ValidateEntities();
        }
    }

    internal class buildGcodeHandler : System.Windows.Input.ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.BuildGcode();
        }
    }
}
