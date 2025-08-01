using CPPacker;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CPPacker
{

    interface ISerialNode
    {
        object ToJsonModel();
    }

    interface IDeserialNode
    {
        object ToViewModel();
    }

    public class PackProject : CPLib.NotifyObject, ISerialNode
    {
        private ObservableDictionary<string, object> _CacheData = new ObservableDictionary<string, object>();
        public ObservableDictionary<string, object> CacheData
        {
            get { return _CacheData; }
            private set
            {
                if (_CacheData != value)
                {
                    _CacheData = value;
                    this.OnPropertyChanged("CacheData");
                }
            }
        }

        private bool _IsDirty = false;
        public bool IsDirty
        {
            get { return _IsDirty; }
            private set
            {
                if (_IsDirty != value)
                {
                    _IsDirty = value;
                    this.OnPropertyChanged("IsDirty");
                }
            }
        }

        public PackProject()
        {
            this.InsertCommand = new CPLib.RelayCommand<object>(this.InsertCommandExecute, this.CanInsertCommandExecute);
            this.RemoveCommand = new CPLib.RelayCommand<object>(this.RemoveCommandExecute, this.CanRemoveCommandExecute);
            this.SaveCommand = new CPLib.RelayCommand<object>(this.SaveCommandExecute, this.CanSaveCommandExecute);
            this.CloseCommand = new CPLib.RelayCommand<object>(this.CloseCommandExecute, this.CanCloseCommandExecute);

            this.Steps.CollectionChanged += Steps_CollectionChanged;
        }

        private void Steps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (StepBase v in e.OldItems)
                {
                    v.OnDetaching();
                    v.Parent = null;
                    v.PropertyChanged -= V_PropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (StepBase v in e.NewItems)
                {
                    v.Parent = this;
                    v.OnAttached();
                    v.PropertyChanged += V_PropertyChanged;
                }
            }
        }

        private void V_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

        }

        private string _File;
        public string File
        {
            get { return _File; }
            set
            {
                if (_File != value)
                {
                    _File = value;
                    this.OnPropertyChanged("File");
                }
            }
        }


        private ObservableCollection<StepBase> _Preset = new ObservableCollection<StepBase>();
        public ObservableCollection<StepBase> Preset
        {
            get { return _Preset; }
            private set
            {
                if (_Preset != value)
                {
                    _Preset = value;
                    this.OnPropertyChanged("Preset");
                }
            }
        }


        private ObservableCollection<StepBase> _Steps = new ObservableCollection<StepBase>();

        public ObservableCollection<StepBase> Steps
        {
            get { return _Steps; }
            private set
            {
                if (_Steps != value)
                {
                    _Steps = value;
                    this.OnPropertyChanged("Steps");
                }
            }
        }

        public ICommand InsertCommand { get; set; }
        void InsertCommandExecute(object parameter)
        {
            EnumStepType typ = (EnumStepType)parameter;
            switch (typ)
            {
                case EnumStepType.SignFile:
                    this.Steps.Add(new SignFileStep());
                    break;
                case EnumStepType.UpdateDriverInfo:
                    this.Steps.Add(new UpdateDriverInfoStep());
                    break;
                case EnumStepType.UpdateVersionInfo:
                    this.Steps.Add(new UpdateVersionInfoStep());
                    break;
                case EnumStepType.LoadVersion:
                    this.Steps.Add(new LoadVersionStep());
                    break;
                case EnumStepType.LoadOemInfo:
                    this.Steps.Add(new LoadOemStep());
                    break;

            }
        }
        bool CanInsertCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveCommand { get; set; }
        void RemoveCommandExecute(object parameter)
        {
            IList lst = parameter as IList;
            if (lst != null)
            {
                foreach (var v in lst.OfType<StepBase>().ToArray())
                {
                    this.Steps.Remove(v);
                }
            }
        }
        bool CanRemoveCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand SaveCommand { get; set; }
        void SaveCommandExecute(object parameter)
        {
            this.TrySaveProject();
        }
        bool CanSaveCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand CloseCommand { get; set; }
        void CloseCommandExecute(object parameter)
        {
            this.TryCloseProject();
        }
        bool CanCloseCommandExecute(object parameter)
        {
            return true;
        }

        public bool TryCloseProject()
        {
            try
            {
                if (this.IsDirty)
                {
                    string msg = $"你想将更改保存到{this.File ?? "未标题"}吗？";
                    var result = MessageBox.Show(msg, null, MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Yes)
                    {
                        //需要保存
                        if (!this.TrySaveProject())
                            throw new OperationCanceledException();
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        //不需要保存
                        return true;
                    }
                    else
                    {
                        //取消操作
                        throw new OperationCanceledException();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TrySaveProject()
        {
            try
            {
                if (string.IsNullOrEmpty(this.File))
                {
                    SaveFileDialog ofd = new SaveFileDialog()
                    {
                        Filter = "*.ppro|*.ppro"
                    };
                    if (ofd.ShowDialog() == true)
                    {
                        this.SerialProject(ofd.FileName);
                        this.File = ofd.FileName;
                    }
                    else
                    {
                        throw new OperationCanceledException();
                    }
                }
                else
                {
                    this.SerialProject(this.File);
                }

                this.IsDirty = false;
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        public void SerialProject(string filename)
        {
            var node = this.ToJsonModel();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, // 自动在需要时添加类型信息
                Formatting = Formatting.Indented // 格式化输出，便于阅读
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(node, settings);

            System.IO.File.WriteAllText(filename, json);
        }

        public static PackProject LoadProject(string filename)
        {

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, // 自动在需要时添加类型信息
                Formatting = Formatting.Indented // 格式化输出，便于阅读
            };

            var json = System.IO.File.ReadAllText(filename);

            var pNode = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonNode.PackProjectNode>(json, settings);

            var res = pNode.ToViewModel() as PackProject;
            res.File = filename;

            return res;

        }

        public object ToJsonModel()
        {
            return new CPPacker.JsonNode.PackProjectNode
            {
                Steps = this.Steps.Select(t => t.ToJsonModel()).OfType<JsonNode.StepNodeBase>().ToArray(),
            };
        }

    }


    public abstract class StepBase : CPLib.NotifyObject, ISerialNode
    {

        private PackProject _Parent;
        public PackProject Parent
        {
            get { return _Parent; }
            set
            {
                if (_Parent != value)
                {
                    _Parent = value;
                    this.OnPropertyChanged("Parent");
                }
            }
        }


        public virtual void OnAttached()
        {

        }

        public virtual void OnDetaching()
        {

        }

        public abstract object ToJsonModel();
    }


    public class SignFileStep : StepBase
    {
        private string _SignAcFilePath;
        public string SignAcFilePath
        {
            get { return _SignAcFilePath; }
            set
            {
                if (_SignAcFilePath != value)
                {
                    _SignAcFilePath = value;
                    this.OnPropertyChanged("SignAcFilePath");
                }
            }
        }

        private string _SignTimestampUrl;
        public string SignTimestampUrl
        {
            get { return _SignTimestampUrl; }
            set
            {
                if (_SignTimestampUrl != value)
                {
                    _SignTimestampUrl = value;
                    this.OnPropertyChanged("SignTimestampUrl");
                }
            }
        }

        private ObservableCollection<string> _Files = new ObservableCollection<string>();

        public ObservableCollection<string> Files
        {
            get { return _Files; }
            private set
            {
                if (_Files != value)
                {
                    _Files = value;
                    this.OnPropertyChanged("Files");
                }
            }
        }

        public SignFileStep()
        {
            this.AddFileCommand = new CPLib.RelayCommand<object>(this.AddFileCommandExecute, this.CanAddFileCommandExecute);
            this.RemoveFileCommand = new CPLib.RelayCommand<object>(this.RemoveFileCommandExecute, this.CanRemoveFileCommandExecute);
            this.SelectAcFileCommand = new CPLib.RelayCommand<object>(this.SelectAcFileCommandExecute, this.CanSelectAcFileCommandExecute);
        }

        public ICommand AddFileCommand { get; set; }
        void AddFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = true
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (var v in ofd.FileNames)
                {
                    if (this.Files.FirstOrDefault(t => string.Compare(v, t, StringComparison.OrdinalIgnoreCase) == 0) == null)
                    {
                        this.Files.Add(v);
                    }

                }
            }
        }
        bool CanAddFileCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileCommand { get; set; }
        void RemoveFileCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;

            if (lst != null)
            {
                foreach (var v in lst.OfType<object>().ToArray())
                {
                    string file = Convert.ToString(v);
                    this.Files.Remove(file);
                }
            }
        }
        bool CanRemoveFileCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand SelectAcFileCommand { get; set; }
        void SelectAcFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "*.*|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                this.SignAcFilePath = ofd.FileName;
            }
        }
        bool CanSelectAcFileCommandExecute(object parameter)
        {
            return true;
        }

        public override object ToJsonModel()
        {
            return new CPPacker.JsonNode.SignFileStepNode
            {
                Files = this.Files.ToArray(),
                SignAcFilePath = this.SignAcFilePath,
                SignTimestampUrl = this.SignTimestampUrl
            };
        }
    }


    public class UpdateVersionInfoStep : StepBase
    {
        private ObservableCollection<InfoItem> _Files = new ObservableCollection<InfoItem>();

        public ObservableCollection<InfoItem> Files
        {
            get { return _Files; }
            private set
            {
                if (_Files != value)
                {
                    _Files = value;
                    this.OnPropertyChanged("Files");
                }
            }
        }


        private ObservableCollection<InfoDetail> _CommonDetail = new ObservableCollection<InfoDetail>();
        public ObservableCollection<InfoDetail> CommonDetail
        {
            get { return _CommonDetail; }
            private set
            {
                if (_CommonDetail != value)
                {
                    _CommonDetail = value;
                    this.OnPropertyChanged("CommonDetail");
                }
            }
        }


        public UpdateVersionInfoStep()
        {
            this.AddFileCommand = new CPLib.RelayCommand<object>(this.AddFileCommandExecute, this.CanAddFileCommandExecute);
            this.RemoveFileCommand = new CPLib.RelayCommand<object>(this.RemoveFileCommandExecute, this.CanRemoveFileCommandExecute);
            this.AddFileDetailCommand = new CPLib.RelayCommand<object>(this.AddFileDetailCommandExecute, this.CanAddFileDetailCommandExecute);
            this.RemoveFileDetailCommand = new CPLib.RelayCommand<object>(this.RemoveFileDetailCommandExecute, this.CanRemoveFileDetailCommandExecute);
        }

        public ICommand AddFileCommand { get; set; }
        void AddFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = true
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (var v in ofd.FileNames)
                {
                    if (this.Files.FirstOrDefault(t => string.Compare(v, t.File, StringComparison.OrdinalIgnoreCase) == 0) == null)
                    {

                        this.Files.Add(new InfoItem() { File = v });
                    }

                }
            }
        }
        bool CanAddFileCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileCommand { get; set; }
        void RemoveFileCommandExecute(object parameter)
        {

            System.Collections.IList lst = parameter as System.Collections.IList;

            if (lst != null)
            {
                foreach (InfoItem v in lst.OfType<InfoItem>().ToArray())
                {

                    this.Files.Remove(v);
                }
            }
        }
        bool CanRemoveFileCommandExecute(object parameter)
        {
            return true;

        }

        public ICommand AddFileDetailCommand { get; set; }
        void AddFileDetailCommandExecute(object parameter)
        {

            this.CommonDetail.Add(new InfoDetail());


        }
        bool CanAddFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileDetailCommand { get; set; }
        void RemoveFileDetailCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;
            if (lst != null)
            {
                foreach (var v in lst.OfType<InfoDetail>().ToArray())
                {
                    this.CommonDetail.Remove(v);

                }
            }
        }
        bool CanRemoveFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public override object ToJsonModel()
        {
            return new CPPacker.JsonNode.UpdateVersionInfoStepNode
            {
                Files = this.Files.Select(t => t.ToJsonModel()).OfType<JsonNode.InfoItemNode>().ToArray(),
                CommonDetail = this.CommonDetail.Select(t => t.ToJsonModel()).OfType<JsonNode.InfoDetailNode>().ToArray()

            };
        }
    }

    public class UpdateDriverInfoStep : StepBase
    {
        private ObservableCollection<InfoItem> _Files = new ObservableCollection<InfoItem>();
        public ObservableCollection<InfoItem> Files
        {
            get { return _Files; }
            private set
            {
                if (_Files != value)
                {
                    _Files = value;
                    this.OnPropertyChanged("Files");
                }
            }
        }

        private ObservableCollection<InfoDetail> _CommonDetail = new ObservableCollection<InfoDetail>();
        public ObservableCollection<InfoDetail> CommonDetail
        {
            get { return _CommonDetail; }
            private set
            {
                if (_CommonDetail != value)
                {
                    _CommonDetail = value;
                    this.OnPropertyChanged("CommonDetail");
                }
            }
        }

        public UpdateDriverInfoStep()
        {
            this.AddFileCommand = new CPLib.RelayCommand<object>(this.AddFileCommandExecute, this.CanAddFileCommandExecute);
            this.RemoveFileCommand = new CPLib.RelayCommand<object>(this.RemoveFileCommandExecute, this.CanRemoveFileCommandExecute);
            this.AddFileDetailCommand = new CPLib.RelayCommand<object>(this.AddFileDetailCommandExecute, this.CanAddFileDetailCommandExecute);
            this.RemoveFileDetailCommand = new CPLib.RelayCommand<object>(this.RemoveFileDetailCommandExecute, this.CanRemoveFileDetailCommandExecute);
        }

        public ICommand AddFileCommand { get; set; }
        void AddFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = true
            };
            if (ofd.ShowDialog() == true)
            {
                foreach (var v in ofd.FileNames)
                {
                    if (this.Files.FirstOrDefault(t => string.Compare(v, t.File, StringComparison.OrdinalIgnoreCase) == 0) == null)
                    {

                        this.Files.Add(new InfoItem() { File = v });
                    }

                }
            }
        }
        bool CanAddFileCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileCommand { get; set; }
        void RemoveFileCommandExecute(object parameter)
        {

            System.Collections.IList lst = parameter as System.Collections.IList;

            if (lst != null)
            {
                foreach (InfoItem v in lst.OfType<InfoItem>().ToArray())
                {

                    this.Files.Remove(v);
                }
            }
        }
        bool CanRemoveFileCommandExecute(object parameter)
        {
            return true;

        }

        public ICommand AddFileDetailCommand { get; set; }
        void AddFileDetailCommandExecute(object parameter)
        {

            this.CommonDetail.Add(new InfoDetail());


        }
        bool CanAddFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileDetailCommand { get; set; }
        void RemoveFileDetailCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;
            if (lst != null)
            {
                foreach (var v in lst.OfType<InfoDetail>().ToArray())
                {
                    this.CommonDetail.Remove(v);

                }
            }
        }
        bool CanRemoveFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public override object ToJsonModel()
        {
            return new JsonNode.UpdateDriverInfoStepNode
            {
                CommonDetail = this.CommonDetail.Select(t => t.ToJsonModel()).OfType<JsonNode.InfoDetailNode>().ToArray(),
                Files = this.Files.Select(t => t.ToJsonModel()).OfType<JsonNode.InfoItemNode>().ToArray()
            };
        }
    }

    public class LoadVersionStep : StepBase
    {
        private string _VersionFrom;
        public string VersionFrom
        {
            get { return _VersionFrom; }
            set
            {
                if (_VersionFrom != value)
                {
                    _VersionFrom = value;
                    this.OnPropertyChanged("VersionFrom");
                }
            }
        }

        private Version _Version;
        public Version Version
        {
            get { return _Version; }
            set
            {
                if (_Version != value)
                {
                    _Version = value;
                    this.OnPropertyChanged("Version");
                }
            }
        }


        public LoadVersionStep()
        {
            this.SelectFileCommand = new CPLib.RelayCommand<object>(this.SelectFileCommandExecute, this.CanSelectFileCommandExecute);
        }

        public ICommand SelectFileCommand { get; set; }
        void SelectFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "*.dll|*.dll|*.exe|*.exe",
            };
            if (ofd.ShowDialog() == true)
            {
                this.VersionFrom = ofd.FileName;
            }
        }
        bool CanSelectFileCommandExecute(object parameter)
        {
            return true;
        }


        protected override void OnPropertyChangedOverride(string propertyName)
        {
            base.OnPropertyChangedOverride(propertyName);

            switch (propertyName)
            {
                case nameof(this.VersionFrom):
                    this.OnVersionFromChanged();
                    break;
                case nameof(this.Version):
                    this.OnVersionChanged();
                    break;
            }
        }


        void OnVersionFromChanged()
        {
            try
            {
                System.Diagnostics.FileVersionInfo fileVerInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(this.VersionFrom);
                //版本号显示为“主版本号.次版本号.内部版本号.专用部件号”。
                var version = String.Format("{0}.{1}.{2}.{3}", fileVerInfo.FileMajorPart, fileVerInfo.FileMinorPart, fileVerInfo.FileBuildPart, fileVerInfo.FilePrivatePart);

                this.Version = new Version(version);

            }
            catch (Exception err)
            {
                this.Version = null;
            }
        }

        void OnVersionChanged()
        {
            if (this.Parent != null)
            {
                this.Parent.CacheData["VER"] = this.Version;
            }
        }

        public override object ToJsonModel()
        {
            return new JsonNode.LoadVersionStepNode
            {
                VersionFrom = this.VersionFrom
            };
        }

        public override void OnAttached()
        {
            this.Parent.CacheData["VER"] = this.Version;
        }

        public override void OnDetaching()
        {
            this.Parent.CacheData.Remove("VER");
        }
    }

    public class LoadOemStep : StepBase
    {

        private string _OemDataFile;
        public string OemDataFile
        {
            get { return _OemDataFile; }
            set
            {
                if (_OemDataFile != value)
                {
                    _OemDataFile = value;
                    this.OnPropertyChanged("OemDataFile");
                }
            }
        }


        private string _OemInfoRsaPublicKey = @"-----BEGIN PUBLIC KEY-----
MIICIjANBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAmpc854559Bp+G44JNS2c
qCqYMc0B27nAiiq39XpQOAmy0zfUpdxsHlH2fa0Yd4XZLBDHypZcxPPImH8QD24g
dZhF0ED0E+KCgJKzENmfHcgdqsQGy8tLjWdQ/8MWr1eeozMFnOxR/ZE+t6VNoBtf
SAlyiFvRamyWriMmLQeqj3TRp3FRuOZU8hZhaRtYZEfoBxTa9VSysce8JYC5cw+o
3Q9YgR2uHQnXiY6xQa56uPs4kiYEMJ7QhkgxBe0z7Jp6HLKpHiTsLg2Fb6B5k0MY
cubj1haL7c9QWzC1/6rTuAsgSudD0iMt3D6wdyonnuXr/4s5A9u9oMoILU8Ou7Bp
ebxPD1iUjOxpBQhd+Nn9Xf6f5oWM6ZJd1Zd7tfNfZa3LSUFOseH2fhJFiVGi0NiH
4IlVrqhxFWV8WU400vT0EF7FKijicvuI6OfOFZz9iLADFG+H5I+Jjq6MEpw8RWPU
kO3aoKUV4P6y/EEEcTAdgWr7J0brmHg14woiP0pj5Eyc3CsrL0glLucuc9FC0gGW
2b9Rl6tWQIqIsEmazkNPBvMQQbnoabQwIYM5A2yugIN/IPv4HOyfWkpPoTRSFSVT
ikUr9b6B4pX+7gpgZgAhVqXwEcjW0dTLmSCu34TAHosVN9oaZhGcu6tSN4jdBn5W
7Fl78qVzg4CeYJ6zxH7bxGUCAwEAAQ==
-----END PUBLIC KEY-----";
        public string OemInfoRsaPublicKey
        {
            get { return _OemInfoRsaPublicKey; }
            set
            {
                if (_OemInfoRsaPublicKey != value)
                {
                    _OemInfoRsaPublicKey = value;
                    this.OnPropertyChanged("OemInfoRsaPublicKey");
                }
            }
        }


        private Dictionary<string, string> _OemCache;
        public Dictionary<string, string> OemCache
        {
            get { return _OemCache; }
            set
            {
                if (_OemCache != value)
                {
                    _OemCache = value;
                    this.OnPropertyChanged("OemCache");
                }
            }
        }



        public LoadOemStep()
        {
            this.SelectFileCommand = new CPLib.RelayCommand<object>(this.SelectFileCommandExecute, this.CanSelectFileCommandExecute);
        }

        public ICommand SelectFileCommand { get; set; }
        void SelectFileCommandExecute(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "*.dat|*.dat",
            };
            if (ofd.ShowDialog() == true)
            {
                this.OemDataFile = ofd.FileName;
            }
        }
        bool CanSelectFileCommandExecute(object parameter)
        {
            return true;
        }


        protected override void OnPropertyChangedOverride(string propertyName)
        {
            base.OnPropertyChangedOverride(propertyName);

            switch (propertyName)
            {
                case nameof(this.OemDataFile):
                    this.OnOemDataFileChanged();
                    break;
                case nameof(this.OemInfoRsaPublicKey):
                    this.OnOemInfoRsaPublicKeyChanged();
                    break;
                case nameof(this.OemCache):
                    this.OnOemCacheChanged();
                    break;
            }
        }

        void OnOemDataFileChanged()
        {
            this.LoadOem();
        }

        void OnOemInfoRsaPublicKeyChanged()
        {
            this.LoadOem();
        }

        void OnOemCacheChanged()
        {
            if (this.Parent != null)
            {
                this.Parent.CacheData["OEM"] = this.OemCache;
            }
        }



        void LoadOem()
        {

            try
            {

                OemInfoHelper.Initialize(this.OemDataFile, this.OemInfoRsaPublicKey);

                Dictionary<string, string> dictCache = new Dictionary<string, string>();

                var values = Enum.GetValues(typeof(OemInfoType));

                foreach (OemInfoType v in values)
                {
                    var data = OemInfoHelper.GetOemInfo<string>(v, false);

                    dictCache.Add(v.ToString(), data);
                }

                this.OemCache = dictCache;
            }
            catch
            {
                this.OemCache = null;
            }
        }

        public override object ToJsonModel()
        {
            return new JsonNode.LoadOemStepNode
            {
                OemDataFile = this.OemDataFile,
                OemInfoRsaPublicKey = this.OemInfoRsaPublicKey
            };
        }

        public override void OnAttached()
        {
            base.OnAttached();
            this.Parent.CacheData["OEM"] = this.OemCache;
        }

        public override void OnDetaching()
        {
            this.Parent.CacheData.Remove("OEM");

        }
    }

    public class InfoItem : CPLib.NotifyObject, ISerialNode
    {
        private string _File;
        public string File
        {
            get { return _File; }
            set
            {
                if (_File != value)
                {
                    _File = value;
                    this.OnPropertyChanged("File");
                }
            }
        }

        private ObservableCollection<InfoDetail> _Info = new ObservableCollection<InfoDetail>();
        public ObservableCollection<InfoDetail> Info
        {
            get { return _Info; }
            private set
            {
                if (_Info != value)
                {
                    _Info = value;
                    this.OnPropertyChanged("Info");
                }
            }
        }


        public InfoItem()
        {
            this.AddFileDetailCommand = new CPLib.RelayCommand<object>(this.AddFileDetailCommandExecute, this.CanAddFileDetailCommandExecute);
            this.RemoveFileDetailCommand = new CPLib.RelayCommand<object>(this.RemoveFileDetailCommandExecute, this.CanRemoveFileDetailCommandExecute);
        }

        public ICommand AddFileDetailCommand { get; set; }
        void AddFileDetailCommandExecute(object parameter)
        {

            this.Info.Add(new InfoDetail());


        }
        bool CanAddFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public ICommand RemoveFileDetailCommand { get; set; }
        void RemoveFileDetailCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;
            if (lst != null)
            {
                foreach (var v in lst.OfType<InfoDetail>().ToArray())
                {
                    this.Info.Remove(v);

                }
            }
        }
        bool CanRemoveFileDetailCommandExecute(object parameter)
        {
            return true;
        }

        public object ToJsonModel()
        {
            return new CPPacker.JsonNode.InfoItemNode
            {
                File = this.File,
                Info = this.Info.Select(t => t.ToJsonModel()).OfType<JsonNode.InfoDetailNode>().ToArray()
            };
        }
    }

    public class InfoDetail : CPLib.NotifyObject, ISerialNode
    {
        public InfoDetail()
        {
            this.Value = new FormattedStringValue();
        }

        private string _Key;
        public string Key
        {
            get { return _Key; }
            set
            {
                if (_Key != value)
                {
                    _Key = value;
                    this.OnPropertyChanged("Key");
                }
            }
        }


        private FormattedStringValue _Value;
        public FormattedStringValue Value
        {
            get { return _Value; }
            set
            {
                if (_Value != value)
                {
                    _Value = value;
                    this.OnPropertyChanged("Value");
                }
            }
        }

        public object ToJsonModel()
        {
            return new JsonNode.InfoDetailNode
            {
                Key = this.Key,
                Value = this.Value.ToJsonModel() as JsonNode.FormattedStringValueNode
            };
        }
    }


    public class FormattedStringValue : CPLib.NotifyObject, ISerialNode
    {
        private string _Format;
        public string Format
        {
            get { return _Format; }
            set
            {
                if (_Format != value)
                {
                    _Format = value;
                    this.OnPropertyChanged("Format");
                }
            }
        }

        public ObservableCollection<FormatArgItem> Args { get; private set; } = new ObservableCollection<FormatArgItem>();


        public FormattedStringValue()
        {
            this.AddArgCommand = new CPLib.RelayCommand<object>(this.AddArgCommandExecute, this.CanAddArgCommandExecute);
            this.RemoveArgCommand = new CPLib.RelayCommand<object>(this.RemoveArgCommandExecute, this.CanRemoveArgCommandExecute);
        }

        public ICommand AddArgCommand { get; set; }
        void AddArgCommandExecute(object parameter)
        {
            var argType = (EnumFormatArgType)parameter;

            this.Args.Add(new FormatArgItem() { Type = argType });
        }
        bool CanAddArgCommandExecute(object parameter)
        {
            return true;
        }


        public ICommand RemoveArgCommand { get; set; }
        void RemoveArgCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;
            if (lst != null)
            {
                foreach (var v in lst.OfType<FormatArgItem>().ToArray())
                {
                    this.Args.Remove(v);
                }
            }
        }
        bool CanRemoveArgCommandExecute(object parameter)
        {
            System.Collections.IList lst = parameter as System.Collections.IList;
            return lst != null && lst.Count > 0;
        }

        public object ToJsonModel()
        {
            return new JsonNode.FormattedStringValueNode
            {
                Format = this.Format,
                Args = this.Args.Select(t => t.ToJsonModel()).OfType<JsonNode.FormatArgItemNode>().ToArray()
            };
        }
    }


    public class FormatArgItem : CPLib.NotifyObject, ISerialNode
    {
        private EnumFormatArgType _Type = EnumFormatArgType.Constant;
        public EnumFormatArgType Type
        {
            get { return _Type; }
            set
            {
                if (_Type != value)
                {
                    _Type = value;
                    this.OnPropertyChanged("Type");
                }
            }
        }



        private string _Extension;
        public string Extension
        {
            get { return _Extension; }
            set
            {
                if (_Extension != value)
                {
                    _Extension = value;
                    this.OnPropertyChanged("Extension");
                }
            }
        }

        public object ToJsonModel()
        {
            return new JsonNode.FormatArgItemNode
            {
                Type = this.Type,
                Extension = this.Extension
            };
        }
    }


    public enum EnumStepType
    {
        SignFile,
        UpdateVersionInfo,
        UpdateDriverInfo,
        LoadVersion,
        LoadOemInfo
    }

    public enum EnumValueProviderType
    {
        BaseStringValue,

        FormattedStringValue,
    }

    public enum EnumFormatArgType
    {
        Constant,
        OemField,
        DateTime,
        Version,
        FileName,
        FileNameWithExtension
    }


}
