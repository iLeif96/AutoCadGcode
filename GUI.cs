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

namespace AutoCadGcode
{
    public class GUI
    {
        public UserEntity activeEntity = null;
        public int order = -1;
        
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

        protected RibbonButton validateEntityesButton = new RibbonButton();
        private RibbonPanelSource rbValidatePanelSource = new RibbonPanelSource();
        private RibbonPanel rbValidatePanel = new RibbonPanel();

        private RibbonTab rbTab = new RibbonTab();
        private RibbonControl rbCntrl = ComponentManager.Ribbon;

        public GUI()
        {
            CreateGUI();
            Global.doc.ImpliedSelectionChanged += OnChangeSelectedObject;
        }

        private void OnChangeSelectedObject(object sender, EventArgs e)
        {
            
            PromptSelectionResult res = Global.editor.SelectImplied();
            if (res.Value != null)
            { 
                var list = API.ListFromSelecion(res);
                if (list != null && list.Count > 0)
                    activeEntity = API.GetXData(list).Last<UserEntity>();
            }
            else
                activeEntity = null;

            //if (e.Selection.Count > 0)
            //    this.activeEntity = API.FindUserEntityByObjectId(e.Selection[e.Selection.Count - 1].ObjectId);
            //else
            //    this.activeEntity = null;

            if (activeEntity != null)
            {
                this.setOrderSpinner.Value = activeEntity.properties.Order;
            }
            else
            {
                this.setOrderSpinner.Value = 0;
            }

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

            validateEntityesButton.Id = "_validateEntityesButton";
            validateEntityesButton.CommandHandler = new ValidateEntityesHandler();
            validateEntityesButton.Size = RibbonItemSize.Standard;
            validateEntityesButton.Text = "Валидация";
            validateEntityesButton.ShowText = true;

            rbValidatePanelSource.Title = "Валидация и запуск";
            rbValidatePanelSource.Items.Add(validateEntityesButton);
            rbValidatePanel.Source = rbValidatePanelSource;

            rbTab.Title = "Trace Maker";
            rbTab.Id = "_TraceMakerTab";

            rbTab.Panels.Add(rbPrintablePanel);
            rbTab.Panels.Add(rbNotPrintablePanel);
            rbTab.Panels.Add(rbValidatePanel);


            rbCntrl.Tabs.Add(rbTab);

            rbTab.IsActive = true;
        }

        private void SetOrderHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            order = Convert.ToInt32((sender as RibbonSpinner).TextValue);
        }
    }

    //System.Windows.Media.Imaging.BitmapImage LoadImage(string ImageName)
    //{
    //    return new System.Windows.Media.Imaging.BitmapImage(
    //        new Uri("pack://application:,,,/ACadRibbon;component/" + ImageName + ".png"));
    //}

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

    public class SetOrder : System.Windows.Input.ICommand
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

    public class SetOrderHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            var prop = new Properties();
            prop.Order = Global.gui.order;
            API.SetOrder(prop);
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

    public class ValidateEntityesHandler : System.Windows.Input.ICommand
    {

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object e)
        {
            return true;
        }

        public void Execute(object e)
        {
            API.ValidateEntityes();
        }
    }
}
