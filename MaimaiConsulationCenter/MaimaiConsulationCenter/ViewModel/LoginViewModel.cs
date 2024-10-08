﻿using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using MaimaiConsulationCenter.Common;
using MaimaiConsulationCenter.DataAccess;
using MaimaiConsulationCenter.DataAccess.DataEntity;
using MaimaiConsulationCenter.Model;
using static MaimaiConsulationCenter.View.LoginView;
using System.Runtime.Caching;
using MaimaiConsulationCenter.View;
using System.Windows.Threading;
using System.Net;
using System.IO;

namespace MaimaiConsulationCenter.ViewModel
{

    public class LoginViewModel : NotifyBase
    {
        public static LoginModel LoginModel { get; set; } = new LoginModel("","","");
        public CommandBase CloseWindowCommand { get; set; }
        public CommandBase LoginCommand { get; set; }

        private string _errorMessage = "";
        private Visibility _showProgress = Visibility.Hidden;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; this.DoNotify(); }
        }

        public Visibility ShowProgress
        {
            get { return _showProgress; }
            set {
                _showProgress = value;
                this.DoNotify();
            }
        }

        public LoginViewModel() {
            this.CloseWindowCommand = new CommandBase();
            this.CloseWindowCommand.DoExecute = new Action<object>((o) =>
            {
                (o as Window).Close();
            });
            this.CloseWindowCommand.DoCanExecute = new Func<object,bool>((o) => { return true; });
            this.LoginCommand=new CommandBase();
            this.LoginCommand.DoExecute = new Action<object>(DoLogin);
            this.LoginCommand.DoCanExecute = new Func<object, bool>((o) => { return true; });

        }

        private void DoLoginButtonHidden(Button bt)
        {
            bt.IsEnabled = false ;
            ControlTemplate template = bt.Template;
            Border bd = (Border)template.FindName("LoginButtonTemplateBorder", bt);
            bd.Background=(SolidColorBrush)(new BrushConverter().ConvertFrom("#DDD"));
        }

        private void DoLoginButtonRecover(Button bt)
        {
            bt.IsEnabled = true;
            ControlTemplate template = bt.Template;
            Border bd = (Border)template.FindName("LoginButtonTemplateBorder", bt);
            bd.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#F79645"));
        }

        private void DoLogin(object o)
        {
            Button bt = (Button)(o as Window).FindName("LoginButton");
            DoLoginButtonHidden(bt);
            this.ShowProgress = Visibility.Visible;
            this.ErrorMessage = "";
            if (string.IsNullOrEmpty(LoginModel.UserName))
            {
                this.ErrorMessage = "请输入用户名！";
                this.ShowProgress = Visibility.Collapsed;
                DoLoginButtonRecover(bt);
                return;
            }
/*            if (string.IsNullOrEmpty(LoginModel.Password)) {
                this.ErrorMessage = "请输入密码！";
                this.ShowProgress = Visibility.Collapsed;
                DoLoginButtonRecover(bt);
                return;
            }*/
/*            if (string.IsNullOrEmpty(LoginModel.ValidationCode))
            {
                this.ErrorMessage = "请输入验证码！";
                this.ShowProgress = Visibility.Collapsed;
                DoLoginButtonRecover(bt);
                return;
            }
            if (LoginModel.ValidationCode.ToLower() != "3n3d") { 
                this.ErrorMessage = "验证码错误！";
                this.ShowProgress = Visibility.Collapsed;
                DoLoginButtonRecover(bt);
                return;
            }*/

            Task.Run(new Action(async () =>
            {
                try
                {
                    var user = LocalDataAccess.GetInstance().CheckUserInfo(LoginModel.UserName, LoginModel.Password);
                    if (user == null)
                    {
                        this.ShowProgress = Visibility.Collapsed;
                        throw new Exception("用户模型为空 请调试");
                    }
                    GlobalValues.UserInfo = user;
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //从这里开始缓存所有页面
                        MemoryCache cache = MemoryCache.Default;
                        //ErrorMessage = "正在加载主界面";
                        cache.Add("FirstPageView", new FirstPageView(), new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });
                        //ErrorMessage = "正在加载B50界面";
                        cache.Add("PointsSearchView", new PointsSearchView(), new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });
                        //ErrorMessage = "正在加载全歌曲界面";
                        cache.Add("SongsView", new SongsView(), new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) });
                        //ErrorMessage = "加载完毕";
                    })); 
                    //登录成功
                    Messenger.Default.Send(new LoginSuccessMessage());
                    await Task.Delay(800);
                    //这行代码会关闭登录窗口
                    Application.Current.Dispatcher.Invoke(new Action(() => {(o as Window).DialogResult = true; }));
                }
                catch (Exception ex)
                {
                    this.ShowProgress = Visibility.Collapsed;
                    this.ErrorMessage = ex.Message;
                }
            })).ContinueWith(task =>
            {
                Application.Current.Dispatcher.Invoke(() => {
                    DoLoginButtonRecover(bt);
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    
}
