﻿#pragma checksum "..\..\Settings.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "759E60B8A00D61DBC35CFC8CBA2AB56B19480C23"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using GPU_trace;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace GPU_trace {
    
    
    /// <summary>
    /// Settings
    /// </summary>
    public partial class Settings : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 50 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border border;
        
        #line default
        #line hidden
        
        
        #line 73 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ToolBar;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image SaveButton;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image CloseButton;
        
        #line default
        #line hidden
        
        
        #line 117 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox check_telegram;
        
        #line default
        #line hidden
        
        
        #line 129 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbox_token;
        
        #line default
        #line hidden
        
        
        #line 136 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbox_GPUlim;
        
        #line default
        #line hidden
        
        
        #line 137 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox tbox_MEMlim;
        
        #line default
        #line hidden
        
        
        #line 149 "..\..\Settings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tb_version;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/GPU trace;component/settings.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\Settings.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.border = ((System.Windows.Controls.Border)(target));
            return;
            case 2:
            
            #line 62 "..\..\Settings.xaml"
            ((System.Windows.Controls.Grid)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ToolBar_MouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ToolBar = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.SaveButton = ((System.Windows.Controls.Image)(target));
            
            #line 75 "..\..\Settings.xaml"
            this.SaveButton.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.SaveButton_MouseDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.CloseButton = ((System.Windows.Controls.Image)(target));
            
            #line 97 "..\..\Settings.xaml"
            this.CloseButton.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.CloseButton_MouseDown);
            
            #line default
            #line hidden
            return;
            case 6:
            this.check_telegram = ((System.Windows.Controls.CheckBox)(target));
            
            #line 120 "..\..\Settings.xaml"
            this.check_telegram.Checked += new System.Windows.RoutedEventHandler(this.check_telegram_Checked);
            
            #line default
            #line hidden
            
            #line 121 "..\..\Settings.xaml"
            this.check_telegram.Unchecked += new System.Windows.RoutedEventHandler(this.check_telegram_Unchecked);
            
            #line default
            #line hidden
            return;
            case 7:
            this.tbox_token = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.tbox_GPUlim = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.tbox_MEMlim = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.tb_version = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

