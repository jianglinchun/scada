﻿/*
 * Copyright 2015 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaCommCommon
 * Summary  : TCP client communication layer user interface
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System.Collections.Generic;

namespace Scada.Comm.Layers
{
    /// <summary>
    /// TCP client communication layer user interface
    /// <para>Пользовательский интерфейс слоя связи TCP-клиент</para>
    /// </summary>
    public class CommTcpClientView : CommLayerView
    {
        /// <summary>
        /// Получить наименование слоя связи
        /// </summary>
        public override string Name
        {
            get
            {
                return Localization.UseRussian ? "TCP-клиент" : "TCP client";
            }
        }

        /// <summary>
        /// Получить описание слоя связи
        /// </summary>
        public override string Descr
        {
            get
            {
                return Localization.UseRussian ?
                    "Слой связи TCP-клиент.\n\n" +
                    "Параметры слоя связи:\n" +
                    "IpAddress - удалённый IP-адрес в режиме соединения Shared,\n" +
                    "TcpPort - удалённый TCP-порт по умолчанию,\n" +
                    "Behavior - режим работы слоя связи (Master, Slave),\n" +
                    "ConnMode - режим соединения (Individual, Shared)." :

                    "TCP client communication layer.\n\n" +
                    "Communication layer parameters:\n" +
                    "IpAddress - remote IP address in Shared connection mode,\n" +
                    "TcpPort - remote TCP port by default," +
                    "Behavior - work mode of connection layer (Master, Slave),\n" +
                    "ConnMode - connection mode (Individual, Shared).";
            }
        }

        /// <summary>
        /// Получить информацию о свойствах слоя связи
        /// </summary>
        public override string GetPropsInfo(Dictionary<string, string> layerParams)
        {
            CommTcpClientLogic.Settings defSett = new CommTcpClientLogic.Settings();
            return BuildPropsInfo(layerParams,
                new string[] { "IpAddress", "TcpPort", "Behavior", "ConnMode" },
                new object[] { defSett.IpAddress, defSett.TcpPort, defSett.Behavior, defSett.ConnMode });
        }
    }
}
