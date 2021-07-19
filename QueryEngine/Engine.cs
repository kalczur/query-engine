using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QueryEngine
{
    class Engine
    {
        private Data _data;
        private List<string> _constraintOperators = new List<string> { "=", ">", ">=", "<", "<=" };
        public Engine(Data data)
        {
            this._data = data;
        }

        // Print the results of the query
        public void Query(string sqlString)
        {
            // Regex patterns
            Regex fromRegex = new Regex(@"(?<=from ).*(?= where)");
            Regex whereRegex = new Regex(@"(?<=where ).*(?= select)");
            Regex selectRegex = new Regex(@"(?<=select ).*$");
            Regex selectSplitRegex = new Regex(@"\w+");

            // Cut fragments from the sql string
            string fromString = (fromRegex.Match(sqlString)).ToString();
            string whereString = (whereRegex.Match(sqlString)).ToString();
            string selectWholeString = (selectRegex.Match(sqlString)).ToString();

            // Throw exception if something is missing
            if (fromString == "" || whereString == "" || selectWholeString == "")
                throw new ArgumentException("Missing sql arguments");

            // Collection of SELECT arguments
            MatchCollection selectSplitedMatchCollection = selectSplitRegex.Matches(selectWholeString);

            // List of data chosen by FROM
            IList sourceList = (IList)(_data.GetType().GetField(fromString).GetValue(_data));

            // Print sql query string
            Console.WriteLine(sqlString);
            
            // Foreach data element, condition defined in WHERE is checked
            foreach (var sourceItem in sourceList)
            {
                if (WhereCondition(sourceItem, whereString))
                {
                    Console.WriteLine("----------------------");
                    foreach (Match selectMatch in selectSplitedMatchCollection)
                    {
                        Console.WriteLine(selectMatch.ToString() + " : " + sourceItem.GetType().GetField(selectMatch.ToString()).GetValue(sourceItem));
                    }
                }
            }

        }

        // Return true if whole WHERE condition is true
        // Otherwise return false
        private bool WhereCondition(object sourceItem, string whereString)
        {
            // Result of WHERE condition
            bool whereResult = false;

            // Regex patterns (find deepest bracket)
            Regex bracketRegex = new Regex(@"\([^()]*\)");

            // Remove all whitespace
            whereString = Regex.Replace(whereString, @"\s+", "");

            // Wrapp WHERE string in bracket
            whereString = whereString.Insert(0, "(");
            whereString = whereString.Insert(whereString.Length, ")");

            // Deepest bracket collection
            MatchCollection deepestBracketCollection = bracketRegex.Matches(whereString);

            // While bracket exist in condition
            while (deepestBracketCollection.Count > 0) 
            { 
                foreach (var deepestBracketItem in deepestBracketCollection)
                {
                    // Get string from Match object
                    string deepestBracketItemString = deepestBracketItem.ToString();

                    // Remove bracket
                    string deepestBracketItemCleanString = Regex.Replace(deepestBracketItemString, @"[\(\)]", "");

                    // Get condition result
                    bool conditionResult = BracketCondition(sourceItem, deepestBracketItemCleanString);

                    // Get string form bool conditionResult, and remove brackets
                    string conditionResultString = Regex.Replace(conditionResult.ToString(), @"[\(\)]", "");

                    // Replace condition with result
                    whereString = whereString.Replace(deepestBracketItemString, conditionResultString);
                }
                // Get new deepest bracket collection
                deepestBracketCollection = bracketRegex.Matches(whereString);
            }

            // Convert string value to bool
            whereResult = Convert.ToBoolean(whereString);
            return whereResult;
        }

        // Return true if whole bracket condition is true
        // Otherwise return false
        private bool BracketCondition(object sourceItem, string bracketString)
        {
            // Result of whole bracket condition
            bool bracketResult = false;
            
            // Result of OR condition
            bool orResult = false;

            // AND condition counter
            int andResultCount = 0;

            // Split condition by "or"
            string[] orSplitArray = bracketString.Split(new string[] { "or" }, StringSplitOptions.None);
            
            foreach (string orSplitItem in orSplitArray)
            {
                // Split condition by "and"
                string[] andSplitArray = orSplitItem.Split(new string[] { "and" }, StringSplitOptions.None);
                
                // For each single condition check if its true
                foreach (string andSplitItem in andSplitArray)
                {
                    // If its true, increment "and" condition result counter
                    if (SingleCondition(sourceItem, andSplitItem))
                        andResultCount++;
                }

                // If all of "and" condition are true, the whole "or" condition is true
                if (andResultCount == andSplitArray.Length)
                    orResult = true;
            }

            if (orResult)
                bracketResult = true;

            return bracketResult;
        }

        // Check single condition e.g. "Age>21"
        private bool SingleCondition(object sourceItem, string conditionString)
        {
            // If they have been already checked return immediately
            if (conditionString == "True")
                return true;
            else if (conditionString == "False")
                return false;

            // Regex patterns
            Regex valuesRegex = new Regex(@"[\w']+");
            Regex constraintRegex = new Regex(@"[=><]+");

            // Values collection
            MatchCollection valuesMatchCollection = valuesRegex.Matches(conditionString);

            // Constraint operator 
            string constraintOperator = (constraintRegex.Match(conditionString)).ToString();

            // Left condition value with whitespace
            string leftConditionValueString = (sourceItem.GetType().GetField(valuesMatchCollection[0].ToString()).GetValue(sourceItem)).ToString();
            // Left condition value without whitespace
            leftConditionValueString = Regex.Replace(leftConditionValueString, @"\s+", "");
            
            // Right condition value without whitespace
            string rightConditionValueString = valuesMatchCollection[1].ToString();

            // Throw exception if something is missing
            if (constraintOperator == "" || leftConditionValueString == "" || rightConditionValueString == "")
                throw new ArgumentException("Missing condition arguments");

            // Constraint operator validation
            if (!_constraintOperators.Contains(constraintOperator))
                throw new ArgumentException("Bad constraint operator");

            // Depending on the constraint operator choose what to return
            switch (constraintOperator)
            {
                case "=":
                    if (rightConditionValueString.Contains("'"))
                    {
                        rightConditionValueString = Regex.Replace(rightConditionValueString, "'", "");
                        return leftConditionValueString == rightConditionValueString;
                    }
                    return Convert.ToDouble(leftConditionValueString) == Convert.ToDouble(rightConditionValueString);
                case ">":
                    return Convert.ToDouble(leftConditionValueString) > Convert.ToDouble(rightConditionValueString);
                case ">=":
                    return Convert.ToDouble(leftConditionValueString) >= Convert.ToDouble(rightConditionValueString);
                case "<":
                    return Convert.ToDouble(leftConditionValueString) < Convert.ToDouble(rightConditionValueString);
                case "<=":
                    return Convert.ToDouble(leftConditionValueString) <= Convert.ToDouble(rightConditionValueString);
            }

            return false;
        }
    }
}
