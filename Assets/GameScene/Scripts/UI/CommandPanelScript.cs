using Assets.Scripts.Jobs;
using Assets.Scripts.Utilities.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class CommandPanelScript : MonoBehaviour
    {
        private ToggleGroup toggleGroup;

        public CommandAction GetChosenJob()
        {
            var active = toggleGroup.ActiveToggles().ToList();
            if (active.Count == 0) return default;
            return ToggleJobConverter[active[0].name];
        }

        public void UnselectToggles()
        {
            toggleGroup.SetAllTogglesOff(false);
        }

        private readonly Dictionary<string, CommandAction> ToggleJobConverter = new Dictionary<string, CommandAction>
        {
            { "TB Mine",
                new CommandAction(CommandActionType.JobTemplate, false, p => new MineBlockJob(p), typeof(MineBlockJob))
            },
            { "TB Build",
                new CommandAction(CommandActionType.EditMode, true)
            },
            { "TB Storage",
                new CommandAction(CommandActionType.StorageDesignation, true)
            },
            { "TB Workshop",
                new CommandAction(CommandActionType.WorkshopDesignation, true)
            }
        };

        void Start()
        {
            toggleGroup = GetComponent<ToggleGroup>();
        }
    }

    public struct CommandAction
    {
        public readonly Func<Vector3Int, Job> JobTemplate;
        public readonly Color JobColor;
        public readonly bool OnNormalSide;
        public readonly CommandActionType Type;

        public CommandAction(CommandActionType type, bool onNormalSide, Func<Vector3Int, Job> jobTemplate = null, Type jobType = null)
        {
            Type = type;
            OnNormalSide = onNormalSide;
            JobTemplate = jobTemplate;
            JobColor = Job.JobColorsDictionary.GetReadOnlyValueOrDefault(jobType, Color.white);
            if (type == CommandActionType.JobTemplate ^ jobTemplate != null)
                throw new Exception("JobTemplate job <> jobTemplate == null");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + OnNormalSide.GetHashCode();
                hash = hash * 31 + JobTemplate.GetHashCode();
                hash = hash * 31 + JobColor.GetHashCode();
                return hash;
            }
        }
    }

    public enum CommandActionType
    {
        NULL = 0,
        EditMode,
        JobTemplate,
        StorageDesignation,
        WorkshopDesignation
    }
}
