﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using OpenBYOND.VM;

namespace OpenBYOND.World
{
    public class Tile
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Tile));

        public List<Atom> Atoms = new List<Atom>();

        public string origID;
        public int ID = -1;
    }
}
