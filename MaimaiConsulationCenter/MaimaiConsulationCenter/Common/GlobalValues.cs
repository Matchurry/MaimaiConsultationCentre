﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaimaiConsulationCenter.DataAccess.DataEntity;
using MaimaiConsulationCenter.Model;

namespace MaimaiConsulationCenter.Common
{
    public class GlobalValues
    {
        public static UserEntity UserInfo { get; set; }
        public static ObservableCollection<SongModel.Root> SongsModel { get; set; }
        public static int B15_UI_Id {  get; set; } = 0;
        public static int B35_UI_Id { get; set; } = 0;
    }
}
