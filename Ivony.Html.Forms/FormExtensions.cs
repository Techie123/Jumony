﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using Ivony.Fluent;

namespace Ivony.Html.Forms
{

  /// <summary>
  /// 提供一组关于表单的扩展方法
  /// </summary>
  public static class FormExtensions
  {

    /// <summary>
    /// 尝试将一个HTML元素转换为表单
    /// </summary>
    /// <param name="element">要转换为表单的元素</param>
    /// <returns></returns>
    public static HtmlForm AsForm( this IHtmlElement element )
    {

      if ( element == null )
        throw new ArgumentNullException( "element" );


      return new HtmlForm( element );
    }



    /// <summary>
    /// 获取输入组当前所有选中的值
    /// </summary>
    /// <param name="group">输入组</param>
    /// <returns></returns>
    public static string[] CurrentValues( this IHtmlGroupControl group )
    {
      if ( group == null )
        throw new ArgumentNullException( "group" );

      return group.Items.Where( item => item.Selected ).Select( item => item.Value ).ToArray();
    }




    /// <summary>
    /// 清空输入组所有选中的值
    /// </summary>
    /// <param name="group">输入组</param>
    public static void ClearValues( this IHtmlGroupControl group )
    {

      if ( group == null )
        throw new ArgumentNullException( "group" );

      group.Items.ForAll( item => item.Selected = false );
    }



    /// <summary>
    /// 获取输入组所有可能的值
    /// </summary>
    /// <param name="group">输入组</param>
    /// <returns></returns>
    public static IEnumerable<string> CandidateValues( this IHtmlGroupControl group )
    {

      if ( group == null )
        throw new ArgumentNullException( "group" );

      return group.Items.Select( item => item.Value );
    }



    /// <summary>
    /// 为指定的输入控件设置值
    /// </summary>
    /// <param name="input">要设置值的输入控件</param>
    /// <param name="value">要设置的值</param>
    public static void SetValue( this IHtmlInputControl input, string value )
    {
      if ( input == null )
        throw new ArgumentNullException( "input" );

      var textControl = input as IHtmlTextControl;
      if ( textControl != null )
        SetValue( textControl, value );

      var group = input as IHtmlGroupControl;
      if ( group != null )
        SetValue( group, value );

      throw new NotSupportedException( string.Format( "名为 \"{0}\" 输入控件的类型 \"{1}\" 不受支持。", input.Name, input.GetType().FullName ) );
    }


    /// <summary>
    /// 为指定的文本输入控件设置值
    /// </summary>
    /// <param name="input"></param>
    /// <param name="value"></param>
    public static void SetValue( this IHtmlTextControl input, string value )
    {
      if ( !TrySetValue( input, value ) )
        throw new InvalidOperationException( "因为未知原因导致设置失败" );
    }


    /// <summary>
    /// 为指定的输入组输入控件设置值
    /// </summary>
    /// <param name="input"></param>
    /// <param name="value"></param>
    public static void SetValue( this IHtmlGroupControl input, string value )
    {
      if ( !TrySetValue( input, value ) )
        throw new InvalidOperationException( string.Format( "为名为 \"{0}\" 的输入组控件设置值 \"{1}\" 时失败，输入组可能不支持这个值或值组合。", input.Name, value ) );
    }


    /// <summary>
    /// 尝试为输入组设置一个值
    /// </summary>
    /// <param name="group">输入组</param>
    /// <param name="value">要设置的值</param>
    /// <returns>是否成功</returns>
    public static bool TrySetValue( this IHtmlGroupControl group, string value )
    {

      if ( group == null )
        throw new ArgumentNullException( "group" );

      group.ClearValues();

      if ( string.IsNullOrEmpty( value ) )
        return true;


      if ( group.AllowMultipleSelections )
      {
        var values = value.Split( ',' );
        if ( values.Any( v => group[v] == null ) )
          return false;

        foreach ( var v in values )
        {
          var item = group[v];
          item.Selected = true;
        }

        return true;
      }
      else
      {
        var item = group[value];

        if ( item == null )
          return false;

        item.Selected = true;

        return true;
      }

    }


    /// <summary>
    /// 尝试为文本控件设置一个值
    /// </summary>
    /// <param name="textInput">文本控件</param>
    /// <param name="value">要设置的值</param>
    /// <remarks>对于密码框此方法会设置失败并返回false</remarks>
    /// <returns>是否成功</returns>
    public static bool TrySetValue( this IHtmlTextControl textInput, string value )
    {
      textInput.TextValue = value;

      if ( textInput.TextValue == value )
        return true;

      return false;
    }


    /// <summary>
    /// 尝试为输入控件设置值
    /// </summary>
    /// <param name="input">输入控件</param>
    /// <param name="value">要设置的值</param>
    /// <returns>是否成功</returns>
    /// <remarks>对于密码框或不存在值吻合选项的输入组，此方法会设置失败并返回false</remarks>
    /// <exception cref="System.NotSupportedException">不被支持的输入控件</exception>
    public static bool TrySetValue( this IHtmlInputControl input, string value )
    {

      if ( input == null )
        return false;

      var textControl = input as IHtmlTextControl;
      if ( textControl != null )
        return TrySetValue( textControl, value );

      var group = input as IHtmlGroupControl;
      if ( group != null )
        return TrySetValue( group, value );

      throw new NotSupportedException( string.Format( "名为 \"{0}\" 输入控件的类型 \"{1}\" 不受支持。", input.Name, input.GetType().FullName ) );
    }


    /// <summary>
    /// 获取字符串形式表达的 Value 值
    /// </summary>
    /// <param name="input">输入控件</param>
    /// <returns></returns>
    public static string Value( this IHtmlInputControl input )
    {
      var textInput = input as HtmlInputText;
      if ( textInput != null )
        return textInput.TextValue;

      var group = input as IHtmlGroupControl;
      if ( group != null )
        return string.Join( ",", group.CurrentValues().ToArray() );

      throw new NotSupportedException();
    }


    /// <summary>
    /// 查找与指定元素绑定的 Label
    /// </summary>
    /// <param name="element">要查找绑定的 Label 的元素</param>
    /// <returns>与元素绑定的 Label 集合，如果元素不支持绑定，则返回null</returns>
    public static HtmlLabel[] Labels( this IHtmlFormElement element )
    {
      var control = element as IHtmlFocusableControl;
      if ( control == null )
        return null;

      return Labels( control );
    }


    /// <summary>
    /// 查找与指定元素绑定的 Label
    /// </summary>
    /// <param name="element">要查找绑定的 Label 的元素</param>
    /// <returns>与元素绑定的 Label 集合</returns>
    public static HtmlLabel[] Labels( this IHtmlFocusableControl control )
    {
      return control.Form.FindLabels( control.ElementId );
    }


    /// <summary>
    /// 尝试获取与指定元素绑定的 Label 的文本
    /// </summary>
    /// <param name="element">要查找绑定的 Label 的元素</param>
    /// <returns>绑定的 Label 的文本，如果元素不支持绑定或没找到则返回null</returns>
    public static string LabelText( this IHtmlFormElement element )
    {
      var control = element as IHtmlFocusableControl;
      if ( control == null )
        return null;

      return LabelText( control );
    }



    /// <summary>
    /// 尝试获取与指定元素绑定的 Label 的文本
    /// </summary>
    /// <param name="element">要查找绑定的 Label 的元素</param>
    /// <returns>绑定的 Label 的文本，如果没找到则返回null</returns>
    public static string LabelText( this IHtmlFocusableControl control )
    {
      var labels = Labels( control );
      if ( labels.IsSingle() )
        return labels.First().Text;

      else
        return null;
    }


    /// <summary>
    /// 尝试查找最小包含指定输入控件的容器
    /// </summary>
    /// <param name="inputControl">输入控件</param>
    /// <returns>找到的最小包含容器</returns>
    public static IHtmlContainer FindContainer( this IHtmlInputControl inputControl )
    {
      var inputText = inputControl as HtmlInputText;
      if ( inputText != null )
        return inputText.Element.Container;

      var textarea = inputControl as HtmlTextArea;
      if ( textarea != null )
        return textarea.Element.Container;

      var select = inputControl as HtmlSelect;
      if ( select != null )
        return select.Element.Container;

      var group = inputControl as HtmlButtonGroup;
      if ( group != null )
        return FindContainer( group );

      throw new NotSupportedException();

    }


    private static IHtmlContainer FindContainer( HtmlButtonGroup group )
    {
      var container = group.Items.Select( i => i.Element ).Aggregate( ( item1, item2 ) =>
        {
          return item1.AncestorsAndSelf().FirstOrDefault( e => e.IsAncestorOf( item2 ) || e.Equals( item2 ) );

        } );

      return container;//没有处理文档为公共容器的情况。
    }



  }
}
