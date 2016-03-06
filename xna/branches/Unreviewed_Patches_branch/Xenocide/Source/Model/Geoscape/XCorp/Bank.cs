#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file Bank.cs
* @date Created: 2007/06/03
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;


using ProjectXenocide.Utils;
using ProjectXenocide.Model;
using ProjectXenocide.Model.Geoscape;

using CeGui;
using Xenocide.Resources;


#endregion

namespace ProjectXenocide.Model.Geoscape
{
    /// <summary>
    /// Conceptually, a bank account, with a balance and a tranaction history
    /// </summary>
    [Serializable]
    public class Bank
    {
        /// <summary>
        /// Get funds available for spending
        /// </summary>
        /// <returns></returns>
        public int CurrentBalance { get { return Balances[MonthlyLog.ThisMonth]; } }

        /// <summary>
        /// Current balance formatted as a string for display to user
        /// </summary>
        public string DisplayCurrentBalance { get { return Util.FormatCurrency(CurrentBalance); } }

        /// <summary>
        /// Check if have sufficient funds to purchase an item
        /// <remarks>Will post a dialog if balance is insufficient</remarks>
        /// </summary>
        /// <param name="cost">cost of item</param>
        /// <returns>true if we have sufficient funds</returns>
        public bool CanAfford(int cost)
        {
            if (cost <= CurrentBalance)
            {
                return true;
            }
            else
            {
                Util.ShowMessageBox(Strings.MSGBOX_INSUFFICIENT_FUNDS);
                return false;
            }
        }

        /// <summary>
        /// Record that player sold something at this GeoTime
        /// </summary>
        /// <param name="price">quantity earned</param>
        public void Credit(int price) 
        {
            Debug.Assert(0 <= price);
            balances[MonthlyLog.ThisMonth] += price;
            sales[MonthlyLog.ThisMonth]    += price;
        }

        /// <summary>
        /// Record that player spent money buying something at this GeoTime
        /// </summary>
        /// <param name="price">quantity spent</param>
        public void Debit(int price) 
        {
            Debug.Assert(0 <= price);
            balances[MonthlyLog.ThisMonth]  -= price;
            purchases[MonthlyLog.ThisMonth] += price;
        }

        /// <summary>
        /// Do the start of month accounting
        /// </summary>
        /// <param name="lastMonthsMaintenance">The Maintenance cost for the previous month</param>
        /// <param name="funding">Income we're recieving from countries for this month</param>
        public void StartOfMonth(int lastMonthsMaintenance, int funding)
        {
            Debug.Assert(0 <= lastMonthsMaintenance);
            Debug.Assert(0 <= funding);

            // update balance
            balances[MonthlyLog.LastMonth] -= lastMonthsMaintenance;
            balances[MonthlyLog.ThisMonth] =  balances[MonthlyLog.LastMonth] + funding;

            // set history
            funds[MonthlyLog.ThisMonth]       = funding;
            purchases[MonthlyLog.ThisMonth]   = 0;
            sales[MonthlyLog.ThisMonth]       = 0;
            maintenance[MonthlyLog.LastMonth] = lastMonthsMaintenance;
        }

        #region Fields

        /// <summary>
        /// History of balances for past 11 months (and this month)
        /// </summary>
        public MonthlyLog Balances { get { return balances; } }

        /// <summary>
        /// History of sales for past 11 months (and this month)
        /// </summary>
        public MonthlyLog Sales { get { return sales; } }

        /// <summary>
        /// History of income from countries for past 11 months (and this month)
        /// </summary>
        public MonthlyLog Funds { get { return funds; } } 

        /// <summary>
        /// History of maintenance payements for past 11 months (and this month)
        /// </summary>
        public MonthlyLog Maintenance { get { return maintenance; } }

        /// <summary>
        /// History of purchases for past 11 months (and this month)
        /// </summary>
        public MonthlyLog Purchases { get { return purchases; } } 

        /// <summary>
        /// History of balances for past 11 months (and this month)
        /// </summary>
        private MonthlyLog balances = new MonthlyLog();

        /// <summary>
        /// History of sales for past 11 months (and this month)
        /// </summary>
        private MonthlyLog sales = new MonthlyLog();

        /// <summary>
        /// History of funding from countries for past 11 months (and this month)
        /// </summary>
        private MonthlyLog funds = new MonthlyLog();

        /// <summary>
        /// History of maintenance payements for past 11 months (and this month)
        /// </summary>
        private MonthlyLog maintenance = new MonthlyLog();

        /// <summary>
        /// History of purchases for past 11 months (and this month)
        /// </summary>
        private MonthlyLog purchases = new MonthlyLog();

        #endregion Fields
    }
}
