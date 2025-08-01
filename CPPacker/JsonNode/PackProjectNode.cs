using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CPPacker.JsonNode
{
    public class PackProjectNode:IDeserialNode
    {
        public StepNodeBase[] Steps { get; set; }

        public object ToViewModel()
        {
            var pro = new PackProject();
            foreach(var v in   this.Steps.Select(t => t.ToViewModel()).OfType<StepBase>())
            {
                pro.Steps.Add(v);
            }
            return pro;
        }
    }


    public abstract class StepNodeBase : IDeserialNode
    {
        public abstract object ToViewModel();
    }

    public class SignFileStepNode:StepNodeBase
    {
        public string SignAcFilePath { get; set; }

        public string SignTimestampUrl { get; set; }

        public string[] Files { get; set; }

        public override object ToViewModel()
        {
            var step = new SignFileStep
            {
                SignAcFilePath = this.SignAcFilePath,
                SignTimestampUrl = this.SignTimestampUrl,

            };

            foreach (var v in this.Files)
                step.Files.Add(v);

            return step;
        }
    }

    public class UpdateVersionInfoStepNode: StepNodeBase
    {
        public InfoItemNode[] Files { get; set; }

        public InfoDetailNode[] CommonDetail { get; set; }

        public override object ToViewModel()
        {
            var step= new UpdateVersionInfoStep
            {
            };

            foreach (var v in this.CommonDetail.Select(t => t.ToViewModel()).OfType<InfoDetail>())
                step.CommonDetail.Add(v);

            foreach (var v in this.Files.Select(t => t.ToViewModel()).OfType<InfoItem>())
                step.Files.Add(v);

            return step;

        }
    }

    public class UpdateDriverInfoStepNode : StepNodeBase
    {
        public InfoItemNode[] Files { get; set; }

        public InfoDetailNode[] CommonDetail { get; set; }


        public override object ToViewModel()
        {
            var step= new UpdateDriverInfoStep
            {
            };
            foreach(var v in this.Files.Select(t => t.ToViewModel()).OfType<InfoItem>())
            {
                step.Files.Add(v);
            }

            foreach( var v in this.CommonDetail.Select(t => t.ToViewModel()).OfType<InfoDetail>())
            {
                step.CommonDetail.Add(v);
            }
            return step;
        }
    }

    public class LoadVersionStepNode : StepNodeBase
    {
        public string VersionFrom { get; set; }

        public override object ToViewModel()
        {
            return new LoadVersionStep
            {
                VersionFrom = this.VersionFrom,
            };
        }
    }


    public class LoadOemStepNode : StepNodeBase
    {
        public string OemDataFile { get; set; }

        public string OemInfoRsaPublicKey { get; set; }

        public override object ToViewModel()
        {
            return new LoadOemStep
            {
                 OemDataFile =  this.OemDataFile,
                  OemInfoRsaPublicKey=   this.OemInfoRsaPublicKey,
            };
        }
    }


    public class InfoItemNode:IDeserialNode
    {
        public string File { get; set; }

        public InfoDetailNode[] Info { get; set; }

        public object ToViewModel()
        {
            var item = new InfoItem
            {
                File = this.File,
            };

            foreach (var v in this.Info.Select(t => t.ToViewModel()).OfType<InfoDetail>())
                item.Info.Add(v);

            return item;
        }
    }


    public class InfoDetailNode:IDeserialNode
    {
        public string Key { get; set; }

        public FormattedStringValueNode Value { get; set; }

        public object ToViewModel()
        {
            return new InfoDetail
            {
                Key = this.Key,
                 Value =  this.Value.ToViewModel() as FormattedStringValue
            };
        }
    }


    public class FormattedStringValueNode: IDeserialNode
    {
        public string Format { get; set; }

        public FormatArgItemNode[] Args { get; set; }

        public object ToViewModel()
        {
            var obj= new FormattedStringValue
            {
                Format = this.Format,                
            };

            foreach (var v in this.Args.Select(t => t.ToViewModel()).OfType<FormatArgItem>())
                obj.Args.Add(v);

            return obj;

        }
    }



    public class FormatArgItemNode:IDeserialNode
    {
        public EnumFormatArgType Type { get; set; }
        public string Extension { get; set; }

        public object ToViewModel()
        {
            return new FormatArgItem
            {
                Type = this.Type,
                Extension = this.Extension,
            };
        }
    }
}
