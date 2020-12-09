using System;
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
using Autodesk.AutoCAD.ApplicationServices;

namespace AutoCadGcode 
{
    public class GUI
    {
        public delegate void VirtaulParametersChangedHandler();
        public static event VirtaulParametersChangedHandler VirtaulParametersChangedEvent;

        /// <summary>
        /// Need to reciving information about selected object
        /// </summary>
        public static UserEntity ActiveEntity { get; set; } = null;

        private static Properties _virtualProperies = new Properties();
        /// <summary>
        ///Store last selected object`s properties for display they.
        ///Diiference with activeEntity is that after removing the selection
        ///information is not lost here
        /// </summary>
        public static Properties VirtualProperies
        {
            get { return _virtualProperies; }
            set
            {
                _virtualProperies = value;
                GUI.VirtaulParametersChangedEvent?.Invoke();
            }
        }

        public ButtonClickHandler buttonClickHandler = new ButtonClickHandler();

        private List<RibbonButton> buttonsList = new List<RibbonButton>();
        private List<RibbonSpinner> spinnersList = new List<RibbonSpinner>();

        /***
         * User interface items
         */

        protected RibbonButton setPumpingTrueButton = new RibbonButton();
        protected RibbonButton setPumpingFalseButton = new RibbonButton();
        protected RibbonSpinner setOrderSpinner = new RibbonSpinner();
        protected RibbonButton setOrderButton = new RibbonButton();
        protected RibbonButton setPrintableButton = new RibbonButton();
        protected RibbonPanelSource rbPrintablePanelSource = new RibbonPanelSource();
        protected RibbonRowPanel rbPrintableRowPanel = new RibbonRowPanel();
        protected RibbonPanel rbPrintablePanel = new RibbonPanel();

        protected RibbonSpinner stopAndPumpSpinner = new RibbonSpinner();
        protected RibbonButton stopAndPumpButton = new RibbonButton();
        protected RibbonButton disablePumpingButton = new RibbonButton();
        protected RibbonButton setFirstButton = new RibbonButton();
        protected RibbonButton setLastButton = new RibbonButton();
        protected RibbonButton setNonPrintableButton = new RibbonButton();
        private RibbonPanelSource rbNotPrintablePanelSource = new RibbonPanelSource();
        protected RibbonRowPanel rbNotPrintableRowPanel = new RibbonRowPanel();
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

            setPumpingTrueButton.Name = "setPumpingTrueButton";
            setPumpingTrueButton.Id = "_setPumpingTrueButton";
            setPumpingTrueButton.Size = RibbonItemSize.Standard;
            setPumpingTrueButton.Text = "С бетоном";
            setPumpingTrueButton.ShowText = true;
            buttonsList.Add(setPumpingTrueButton);

            setPumpingFalseButton.Name = "setPumpingFalseButton";
            setPumpingFalseButton.Id = "_setPumpingFalseButton";
            setPumpingFalseButton.Size = RibbonItemSize.Standard;
            setPumpingFalseButton.Text = "Без бетона";
            setPumpingFalseButton.ShowText = true;
            buttonsList.Add(setPumpingFalseButton);

            setOrderSpinner.Name = "setOrderSpinner";
            setOrderSpinner.Id = "_setOrder";
            setOrderSpinner.Size = RibbonItemSize.Standard;
            setOrderSpinner.Text = "Порядок печати";
            setOrderSpinner.ShowText = true;
            setOrderSpinner.Value = 0;
            setOrderSpinner.Minimum = -1;
            setOrderSpinner.Maximum = int.MaxValue;
            setOrderSpinner.Width = 150;
            setOrderSpinner.IsEditable = true;
            setOrderSpinner.IsEnabled = true;
            spinnersList.Add(setOrderSpinner);

            setOrderButton.Name = "setOrderButton";
            setOrderButton.Id = "_setOrderButton";
            setOrderButton.Size = RibbonItemSize.Standard;
            setOrderButton.Text = "Применить";
            setOrderButton.ShowText = true;
            buttonsList.Add(setOrderButton);

            setPrintableButton.Name = "setPrintableButton";
            setPrintableButton.Id = "_setOrderButton";
            setPrintableButton.Size = RibbonItemSize.Large;
            setPrintableButton.Text = "В печатную\nлинию";
            setPrintableButton.ShowText = true;
            buttonsList.Add(setPrintableButton);

            rbPrintablePanelSource.Title = "Параметры печатных линий";
            rbPrintablePanelSource.Items.Add(setPrintableButton);
            rbPrintablePanelSource.Items.Add(new RibbonSeparator());
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
            stopAndPumpSpinner.Name = "stopAndPumpSpinner";
            stopAndPumpSpinner.Id = "_setOrder";
            stopAndPumpSpinner.Size = RibbonItemSize.Standard;
            stopAndPumpSpinner.Text = "Прокачка (мс)";
            stopAndPumpSpinner.ShowText = true;
            stopAndPumpSpinner.Value = 0;
            stopAndPumpSpinner.Minimum = 0;
            stopAndPumpSpinner.Maximum = int.MaxValue;
            stopAndPumpSpinner.Width = 150;
            stopAndPumpSpinner.IsEditable = true;
            stopAndPumpSpinner.IsEnabled = true;
            spinnersList.Add(stopAndPumpSpinner);

            stopAndPumpButton.Name = "stopAndPumpButton";
            stopAndPumpButton.Id = "_stopAndPump";
            stopAndPumpButton.Size = RibbonItemSize.Standard;
            stopAndPumpButton.Text = "Применить";
            stopAndPumpButton.ShowText = true;
            buttonsList.Add(stopAndPumpButton);

            disablePumpingButton.Name = "disablePumpingButton";
            disablePumpingButton.Id = "_disablePumpingButton";
            disablePumpingButton.Size = RibbonItemSize.Standard;
            disablePumpingButton.Text = "Вкл/Выкл подачу";
            disablePumpingButton.ShowText = true;
            buttonsList.Add(disablePumpingButton);

            setFirstButton.Name = "setFirstButton";
            setFirstButton.Id = "_setFirst";
            setFirstButton.Size = RibbonItemSize.Standard;
            setFirstButton.Text = "Первая";
            setFirstButton.ShowText = true;
            buttonsList.Add(setFirstButton);

            setLastButton.Name = "setLastButton";
            setLastButton.Id = "_setLast";
            setLastButton.Size = RibbonItemSize.Standard;
            setLastButton.Text = "Последняя";
            setLastButton.ShowText = true;
            buttonsList.Add(setLastButton);

            setNonPrintableButton.Name = "setNonPrintableButton";
            setNonPrintableButton.Id = "_setNonPrintableButton";
            setNonPrintableButton.Size = RibbonItemSize.Large;
            setNonPrintableButton.Text = "В непечатную\nлинию";
            setNonPrintableButton.ShowText = true;
            buttonsList.Add(setNonPrintableButton);

            rbNotPrintablePanelSource.Title = "Параметры непечатных линий";
            rbNotPrintablePanelSource.Items.Add(setNonPrintableButton);
            rbNotPrintablePanelSource.Items.Add(new RibbonSeparator());
            rbNotPrintablePanelSource.Items.Add(setFirstButton);
            rbNotPrintablePanelSource.Items.Add(new RibbonSeparator());
            rbNotPrintablePanelSource.Items.Add(setLastButton);
            rbNotPrintablePanelSource.Items.Add(new RibbonSeparator());
            rbNotPrintablePanelSource.Items.Add(disablePumpingButton);
            rbNotPrintablePanelSource.Items.Add(new RibbonRowBreak());
            rbNotPrintablePanelSource.Items.Add(stopAndPumpSpinner);
            rbNotPrintablePanelSource.Items.Add(stopAndPumpButton);
            rbNotPrintablePanel.Source = rbNotPrintablePanelSource;

            /**
             * Validation and building
             */

            validateEntitiesButton.Name = "validateEntitiesButton";
            validateEntitiesButton.Id = "_validateEntitiesButton";
            validateEntitiesButton.Size = RibbonItemSize.Standard;
            validateEntitiesButton.Text = "Валидация";
            validateEntitiesButton.ShowText = true;
            buttonsList.Add(validateEntitiesButton);

            buildGcodeButton.Name = "buildGcodeButton";
            buildGcodeButton.Id = "_buildGcodeButton";
            buildGcodeButton.Size = RibbonItemSize.Standard;
            buildGcodeButton.Text = "Построить Gcode";
            buildGcodeButton.ShowText = true;
            buttonsList.Add(buildGcodeButton);

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

            //Work with selected object. TODO: its stop to work sometimes
            Global.DocumentManager.DocumentActivated += (object sender, DocumentCollectionEventArgs e) =>
            {
                Global.Doc.ImpliedSelectionChanged -= OnChangeSelectedObjectHandler;
                Global.Doc.ImpliedSelectionChanged += OnChangeSelectedObjectHandler;
            };
            Global.Doc.ImpliedSelectionChanged += OnChangeSelectedObjectHandler;
            //Global.editor.SelectionAdded += GuiInstance.OnChangeSelectedObjectHandler;
            //Global.editor.SelectionRemoved += GuiInstance.OnChangeSelectedObjectHandler;

            //Action after changing VirtualProperies selected entity
            ///VirtaulParametersChangedEvent += OnVirtualParametersChangedHandler;

            //Set actions for buttons
            buttonClickHandler.ClickEvent += OnButtonClickHandler;
            foreach (RibbonButton rB in buttonsList)
                rB.CommandHandler = buttonClickHandler;


            //Set actions for spinners
            foreach (RibbonSpinner rS in spinnersList)
                rS.ValueChanged += OnSpinnerValueChangeHandler;
        }

        /// <summary>
        /// Function for change ribbon items attributes after incoming properties recived
        /// </summary>
        private void ChangeAttributes()
        {
            //Set attributes that are not affected by deselecting
            //TODO:
            
            //if we have selected entity then set some attributes
            if (ActiveEntity != null) 
            {
                if (VirtualProperies.Printable == true)
                {
                    foreach (var item in rbNotPrintablePanelSource.Items)
                        item.IsVisible = false;
                    foreach (var item in rbPrintablePanelSource.Items)
                        item.IsVisible = true;

                    setOrderSpinner.Value = VirtualProperies.Order;
                    setPrintableButton.IsEnabled = false;
                    setNonPrintableButton.IsEnabled = true;
                }
                else
                {
                    foreach (var item in rbNotPrintablePanelSource.Items)
                        item.IsVisible = true;
                    foreach (var item in rbPrintablePanelSource.Items)
                        item.IsVisible = false;

                    stopAndPumpSpinner.Value = VirtualProperies.StopAndPump;
                    setPrintableButton.IsEnabled = true;
                    setNonPrintableButton.IsEnabled = false;
                }
                setPrintableButton.IsVisible = true;
                setNonPrintableButton.IsVisible = true;
                
            }
            //If we have not selected entity then clear some attributes
            else 
            {
                foreach (var item in rbNotPrintablePanelSource.Items)
                    item.IsVisible = true;
                foreach (var item in rbPrintablePanelSource.Items)
                    item.IsVisible = true;
                setPrintableButton.IsEnabled = true;
                setNonPrintableButton.IsEnabled = true;
                setOrderSpinner.Value = 0;
                stopAndPumpSpinner.Value = 0;
            }
        }

        /***
         * Global handlers that must applyed from main process
         */

        /// <summary>
        /// Then validation changing we need to disable or enable the Build button
        /// </summary>
        public void OnValidationChangingHandler(bool isValidated)
        {
            if (isValidated == true)
                this.buildGcodeButton.IsEnabled = true;
            else
                this.buildGcodeButton.IsEnabled = false;
        }

        /// <summary>
        /// Handler for situation then entities lost focus after changing parameter
        /// </summary>
        public void OnChangeParametersHandler(UserEntity uEntity)
        {
            if (ActiveEntity == null) //Check for selected entity
                VirtualProperies = uEntity.Properties;
        }

        /***
         * Handlers that should apllyed inside the inctance
         */
        /// <summary>
        /// Handler for reciving information about selected entity
        /// </summary>
        public void OnChangeSelectedObjectHandler(object sender, EventArgs e)
        {
            PromptSelectionResult res = Global.Editor.SelectImplied();
            if (res.Value != null)
            {
                var list = API.ListFromSelecion(res);
                if (list != null && list.Count > 0)
                {
                    var tempList = API.GetXData(list);
                    if (tempList.Count == 0)
                        ActiveEntity = null;
                    else
                        ActiveEntity = tempList.Last<UserEntity>();
                }
            }
            else
                ActiveEntity = null;

            if (ActiveEntity != null)
            {
                VirtualProperies = ActiveEntity.Properties;
            }

            ChangeAttributes();
        }
        /// <summary>
        /// Handler should invoked after virtual parameters change; Need for change attributes
        /// </summary>
        private void OnVirtualParametersChangedHandler()
        {
            ChangeAttributes();
        }
        /// <summary>
        /// Handler for all of buttons (for work with ICommand interface)
        /// </summary>
        private void OnButtonClickHandler(RibbonButton rB)
        {
            if (rB == null)
                throw new Exception("There are no Ribbon button inctance for event");

            if (rB.Name == "")
                throw new Exception("There are no name for Ribbon button inctance for event");
            else if (rB.Name == setPrintableButton.Name)
                API.SetPrintable();
            else if (rB.Name == setNonPrintableButton.Name)
                API.SetNonPrintable();
            else if (rB.Name == setPumpingTrueButton.Name)
                API.SetPumpingTrue();
            else if (rB.Name == setPumpingFalseButton.Name)
                API.SetPumpingFalse();
            else if (rB.Name == setOrderButton.Name)
                API.SetOrder(VirtualProperies);
            else if (rB.Name == stopAndPumpButton.Name)
                API.SetStopAndPump(VirtualProperies);
            else if (rB.Name == disablePumpingButton.Name)
                API.SetDisablePumping(VirtualProperies);
            else if (rB.Name == setFirstButton.Name)
                API.SetFirst();
            else if (rB.Name == setLastButton.Name)
                API.SetLast();
            else if (rB.Name == validateEntitiesButton.Name)
                API.ValidateEntities();
            else if (rB.Name == buildGcodeButton.Name)
                API.BuildGcode();
            else
                throw new Exception("For " + rB.Name + " not found any action");
        }
        /// <summary>
        ///Handler for all of spinners (for transfer values from spinners to VirtualProperties)
        /// </summary>
        private void OnSpinnerValueChangeHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RibbonSpinner rS = sender as RibbonSpinner;
            if (rS == null)
                throw new Exception("There are no Ribbon spinner inctance for event");

            if (rS.Name == "")
                throw new Exception("There are no name for Ribbon spinner inctance for event");
            else if (rS.Name == setOrderSpinner.Name)
                VirtualProperies.Order = Convert.ToInt32(rS.TextValue);
            else if (rS.Name == stopAndPumpSpinner.Name)
                VirtualProperies.StopAndPump = Convert.ToInt32(rS.TextValue);
            else
                throw new Exception("For " + rS.Name + " not found any action");
        }

    }
    /// <summary>
    /// Rudiment from AutoCad and WPF. But its need to use buttons clicks
    /// </summary>
    public class ButtonClickHandler : System.Windows.Input.ICommand
    {
#pragma warning disable CS0067 // Событие "ButtonClickHandler.CanExecuteChanged" никогда не используется.
        public event EventHandler CanExecuteChanged;
#pragma warning restore CS0067 // Событие "ButtonClickHandler.CanExecuteChanged" никогда не используется.
        public delegate void ClickHandler(RibbonButton rB);
        public event ClickHandler ClickEvent;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            RibbonButton rB = null;
            try { rB = e as RibbonButton; }
            catch (Exception err) { Global.Editor.WriteMessage("Event in GUI not work: " + err); }

            if (rB != null)
                ClickEvent?.Invoke(rB);
            else
                Global.Editor.WriteMessage("Event for buttons in GUI not work");
        }
    }
}
