using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using System;
using System.Reflection;

namespace Stylet.Avalonia.Xaml
{
    /// <summary>
    /// Holds attached properties relating to various bits of the View which are used by Stylet
    /// </summary>
    public static class View
    {
        /// <summary>
        /// Key which will be used to retrieve the ViewManager associated with the current application, from application's resources
        /// </summary>
        public const string ViewManagerResourceKey = "b9a38199-8cb3-4103-8526-c6cfcd089df7";

        /// <summary>
        /// Initial value of the ActionTarget property.
        /// This can be used as a marker - if the property has this value, it hasn't yet been assigned to anything else.
        /// </summary>
        public static readonly object InitialActionTarget = new object();

        /// <summary>
        /// Get the ActionTarget associated with the given object
        /// </summary>
        /// <param name="obj">Object to fetch the ActionTarget for</param>
        /// <returns>ActionTarget associated with the given object</returns>
        public static object GetActionTarget(AvaloniaObject obj)
        {
            return obj.GetValue(ActionTargetProperty);
        }

        /// <summary>
        /// Set the ActionTarget associated with the given object
        /// </summary>
        /// <param name="obj">Object to set the ActionTarget for</param>
        /// <param name="value">Value to set the ActionTarget to</param>
        public static void SetActionTarget(AvaloniaObject obj, object value)
        {
            obj.SetValue(ActionTargetProperty, value);
        }

        /// <summary>
        /// The object's ActionTarget. This is used to determine what object to call Actions on by the ActionExtension markup extension.
        /// </summary>
        //public static readonly AvaloniaProperty ActionTargetProperty =
        //    AvaloniaProperty.RegisterAttached("ActionTarget", typeof(object), typeof(View), new FrameworkPropertyMetadata(InitialActionTarget, FrameworkPropertyMetadataOptions.Inherits));

        public static readonly AvaloniaProperty ActionTargetProperty =
            //AvaloniaProperty.RegisterAttached<View, IControl, object>("ActionTarget", InitialActionTarget, true);
            AvaloniaProperty.RegisterAttached<IControl, object>("ActionTarget", typeof(View), InitialActionTarget, true);


        /// <summary>
        /// Fetch the ViewModel currently associated with a given object
        /// </summary>
        /// <param name="obj">Object to fetch the ViewModel for</param>
        /// <returns>ViewModel currently associated with the given object</returns>
        public static object GetModel(AvaloniaObject obj)
        {
            return obj.GetValue(ModelProperty);
        }

        /// <summary>
        /// Set the ViewModel currently associated with a given object
        /// </summary>
        /// <param name="obj">Object to set the ViewModel for</param>
        /// <param name="value">ViewModel to set</param>
        public static void SetModel(AvaloniaObject obj, object value)
        {
            obj.SetValue(ModelProperty, value);
        }

        private static readonly object defaultModelValue = new object();

        /// <summary>
        /// Property specifying the ViewModel currently associated with a given object
        /// </summary>
        public static readonly AvaloniaProperty ModelProperty =
            AvaloniaProperty.RegisterAttached<IControl, int>("Model", typeof(View));

        /// <summary>
        /// Helper to set the Content property of a given object to a particular View
        /// </summary>
        /// <param name="targetLocation">Object to set the Content property on</param>
        /// <param name="view">View to set as the object's Content</param>
        public static void SetContentProperty(AvaloniaObject targetLocation, Control view)
        {
            //[TemplatePart("PART_ContentPresenter", typeof(IContentPresenter))]
            var type = targetLocation.GetType();
            var attribute = type.GetCustomAttribute<TemplatePartAttribute>();
            // No attribute? Try a property called 'Content'...
            string propertyName = attribute != null ? attribute.Name : "TemplatePart";
            var property = type.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException(String.Format("Unable to find a Content property on type {0}. Make sure you're using 's:View.Model' on a suitable container, e.g. a ContentControl", type.Name));
            property.SetValue(targetLocation, view);
        }

        static View()
        {
        //    ModelProperty.Changed.Subscribe(d =>
        //    {
        //        //new PropertyMetadata(defaultModelValue, (d, e) =>
        //        //{
                    
        //        //}));
        //    var viewManager = ((Control)d.Sender).TryFindResource(ViewManagerResourceKey) as IViewManager;

        //    if (viewManager == null)
        //    {
        //        if (Execute.InDesignMode)
        //        {
        //            var bindingExpression = BindingOperations.GetBindingExpression(d, ModelProperty);
        //            string text;
        //            if (bindingExpression == null)
        //                text = "View for [Broken Binding]";
        //            else if (bindingExpression.ResolvedSourcePropertyName == null)
        //                text = String.Format("View for child ViewModel on {0}", bindingExpression.DataItem.GetType().Name);
        //            else
        //                text = String.Format("View for {0}.{1}", bindingExpression.DataItem.GetType().Name, bindingExpression.ResolvedSourcePropertyName);
        //            SetContentProperty(d, new System.Windows.Controls.TextBlock() { Text = text });
        //        }
        //        else
        //        {
        //            throw new InvalidOperationException("The ViewManager resource is unassigned. This should have been set by the Bootstrapper");
        //        }
        //    }
        //    else
        //    {
        //        // It appears we can be reset to the default value on destruction
        //        var newValue = e.NewValue == defaultModelValue ? null : e.NewValue;
        //        viewManager.OnModelChanged(d, e.OldValue, newValue);
        //    }
        //})
        }
}
}
