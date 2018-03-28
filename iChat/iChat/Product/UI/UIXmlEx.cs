/*****************************************************************************\
*                                                                             *
* ChartUIXml.cs - Chart xml functions, types, and definitions.                *
*                                                                             *
*               Version 1.00  ����                                          *
*                                                                             *
*               Copyright (c) 2016-2016, iTeam. All rights reserved.      *
*               Created by Lord 2016/12/24.                                   *
*                                                                             *
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Drawing;
using System.Data;

namespace OwLib
{
    /// <summary>
    /// ��Ʊͼ�οؼ�Xml����
    /// </summary>
    public class UIXmlEx:UIXml
    {
        #region Lord 2016/12/24
        private double m_scaleFactor = 1;

        /// <summary>
        /// ��ȡ��������������
        /// </summary>
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set { m_scaleFactor = value; }
        }

        /// <summary>
        /// �����ؼ�
        /// </summary>
        /// <param name="node">�ڵ�</param>
        /// <param name="type">����</param>
        /// <returns>�ؼ�</returns>
        public override ControlA CreateControl(XmlNode node, String type)
        {
            INativeBase native = Native;
            if (type == "barragediv")
            {
                return new BarrageDiv();
            }
            else if (type == "column" || type == "th")
            {
                return new GridColumnEx();
            }
            else if (type == "ribbonbutton")
            {
                return new RibbonButton();
            }
            //͸����ť2
            else if (type == "ribbonbutton2")
            {
                return new RibbonButton2();
            }
            else if (type == "windowex")
            {
                return new WindowEx();
            }
            ControlA control = base.CreateControl(node, type);
            if (control != null)
            {
                control.Font.m_fontFamily = "΢���ź�";
                if (control is CheckBoxA)
                {
                    (control as CheckBoxA).ButtonBackColor = CDraw.PCOLORS_BACKCOLOR8;
                }
            }
            return control;
        }

        /// <summary>
        /// �����˵���
        /// </summary>
        /// <param name="node">�ڵ�</param>
        /// <param name="menu">�˵�</param>
        /// <param name="parentItem">���ڵ�</param>
        protected override void CreateMenuItem(XmlNode node, MenuA menu, MenuItemA parentItem)
        {
            MenuItemA item = new MenuItemA();
            item.Native = Native;
            item.Font = new FONT("΢���ź�", 12, false, false, false);
            SetAttributesBefore(node, item);
            if (parentItem != null)
            {
                parentItem.AddItem(item);
            }
            else
            {
                menu.AddItem(item);
            }
            if (node.ChildNodes != null && node.ChildNodes.Count > 0)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    CreateMenuItem(subNode, menu, item);
                }
            }
            SetAttributesAfter(node, item);
            OnAddControl(item, node);
        }

        /// <summary>
        /// �˳�����
        /// </summary>
        public virtual void Exit()
        {
        }

        /// <summary>
        /// ����XML
        /// </summary>
        /// <param name="xmlPath">XML��ַ</param>
        public virtual void Load(String xmlPath)
        {
        }

        /// <summary>
        /// ��������
        /// </summary>
        public virtual void LoadData()
        {
        }

        /// <summary>
        /// �������ųߴ�
        /// </summary>
        /// <param name="clientSize">�ͻ��˴�С</param>
        public void ResetScaleSize(SIZE clientSize)
        {
            INativeBase native = Native;
            if (native != null)
            {
                ControlHost host = native.Host;
                SIZE nativeSize = native.DisplaySize;
                List<ControlA> controls = native.GetControls();
                int controlsSize = controls.Count;
                for (int i = 0; i < controlsSize; i++)
                {
                    WindowFrameA frame = controls[i] as WindowFrameA;
                    if (frame != null)
                    {
                        WindowEx window = frame.GetControls()[0] as WindowEx;
                        if (window != null && !window.AnimateMoving)
                        {
                            POINT location = window.Location;
                            if (location.x < 10 || location.x > nativeSize.cx - 10)
                            {
                                location.x = 0;
                            }
                            if (location.y < 30 || location.y > nativeSize.cy - 30)
                            {
                                location.y = 0;
                            }
                            window.Location = location;
                        }
                    }
                }
                native.ScaleSize = new SIZE((int)(clientSize.cx * m_scaleFactor), (int)(clientSize.cy * m_scaleFactor));
                native.Update();
            }
        }

        /// <summary>
        /// �Ƿ�ȷ�Ϲر�
        /// </summary>
        /// <returns>������</returns>
        public virtual int Submit()
        {
            return 0;
        }
        #endregion
    }

    /// <summary>
    /// ����XML��չ
    /// </summary>
    public class WindowXmlEx : UIXmlEx
    {
        /// <summary>
        /// ���ÿؼ������¼�
        /// </summary>
        private ControlInvokeEvent m_invokeEvent;

        protected UIXmlEx m_parent;

        /// <summary>
        /// ��ȡ�����ø�����
        /// </summary>
        public UIXmlEx Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }

        protected WindowEx m_window;

        /// <summary>
        /// ��ȡ�����ô���
        /// </summary>
        public WindowEx Window
        {
            get { return m_window; }
        }

        /// <summary>
        /// ��ť����¼�
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="mp">����</param>
        /// <param name="button">��ť</param>
        /// <param name="click">�������</param>
        /// <param name="delta">���ֹ���ֵ</param>
        private void ClickButton(object sender, POINT mp, MouseButtonsA button, int click, int delta)
        {
            if (button == MouseButtonsA.Left && click == 1)
            {
                ControlA control = sender as ControlA;
                if (m_window != null && control == m_window.CloseButton)
                {
                    Close();
                }
            }
        }

        /// <summary>
        /// �ر�
        /// </summary>
        public virtual void Close()
        {
            m_window.Invoke("close");
        }

        /// <summary>
        /// ���ٷ���
        /// </summary>
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                if (m_window != null)
                {
                    m_invokeEvent = null;
                    m_window.Close();
                    m_window.Dispose();
                    m_window = null;
                }
                base.Dispose();
            }
        }

        /// <summary>
        /// ���ؽ���
        /// </summary>
        public virtual void Load(INativeBase native, String xmlName, String windowName)
        {
            Native = native;
            String xmlPath = DataCenter.GetAppPath() + "\\config\\" + xmlName + ".html";
            Script = new GaiaScript(this);
            LoadFile(xmlPath, null);
            m_window = FindControl(windowName) as WindowEx;
            m_invokeEvent = new ControlInvokeEvent(Invoke);
            m_window.RegisterEvent(m_invokeEvent, EVENTID.INVOKE);
            //ע�����¼�
            RegisterEvents(m_window);
        }

        /// <summary>
        /// ���ÿؼ��̷߳���
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="args">����</param>
        private void Invoke(object sender, object args)
        {
            OnInvoke(args);
        }

        /// <summary>
        /// ���ÿؼ��̷߳���
        /// </summary>
        /// <param name="args">����</param>
        public void OnInvoke(object args)
        {
            if (args != null && args.ToString() == "close")
            {
                Dispose();
                Native.Invalidate();
            }
        }

        /// <summary>
        /// ע���¼�
        /// </summary>
        /// <param name="control">�ؼ�</param>
        private void RegisterEvents(ControlA control)
        {
            ControlMouseEvent clickButtonEvent = new ControlMouseEvent(ClickButton);
            List<ControlA> controls = control.GetControls();
            int controlsSize = controls.Count;
            for (int i = 0; i < controlsSize; i++)
            {
                ButtonA button = controls[i] as ButtonA;
                if (button != null)
                {
                    button.RegisterEvent(clickButtonEvent, EVENTID.CLICK);
                }
                RegisterEvents(controls[i]);
            }
        }

        /// <summary>
        /// ��ʾ
        /// </summary>
        public virtual void Show()
        {
            m_window.Location = new POINT(-m_window.Width, -m_window.Height);
            m_window.AnimateShow(false);
            m_window.Invalidate();
        }

        /// <summary>
        /// ��ʾ
        /// </summary>
        public virtual void ShowDialog()
        {
            m_window.Location = new POINT(-m_window.Width, -m_window.Height);
            m_window.AnimateShow(true);
            m_window.Invalidate();
        }
    }

    /// <summary>
    /// ��������չ
    /// </summary>
    public class GridColumnEx : GridColumn
    {
        /// <summary>
        /// �ػ汳������
        /// </summary>
        /// <param name="paint">��ͼ����</param>
        /// <param name="clipRect">�ü�����</param>
        public override void OnPaintBackground(CPaint paint, RECT clipRect)
        {
            int width = Width, height = Height;
            RECT drawRect = new RECT(0, 0, width, height);
            paint.FillGradientRect(CDraw.PCOLORS_BACKCOLOR, CDraw.PCOLORS_BACKCOLOR2, drawRect, 0, 90);
        }
    }
}