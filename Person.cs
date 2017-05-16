using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace StableMarriage
{
    public class Person
    {
        #region Constructor

        public Person()
        {
            _preferenceList = new List<Person>();
        }

        #endregion

        #region Properties

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private List<Person> _preferenceList;
        public List<Person> PreferenceList
        {
            get { return _preferenceList; }
            set { _preferenceList = value; }
        }

        private bool _isEngaged;
        public bool IsEngaged
        {
            get { return _isEngaged; }
            set { _isEngaged = value; }
        }

        public int candidateIndex = 0;

        public Person Match { get; set; }

        #endregion

        #region Methods

        public bool Prefers(Person personObj)
        {
            return PreferenceList.FindIndex(person => person == personObj) < PreferenceList.FindIndex(person => person == Match);
        }

        public Person NextCandidateNotYetProposedTo()
        {
            if (candidateIndex >= PreferenceList.Count) 
                return null;
            return PreferenceList[candidateIndex++];
        }

        public void EngageTo(Person personObj)
        {
            if (personObj.Match != null) 
                personObj.Match.Match = null;
            personObj.Match = this;
            if (Match != null) 
                Match.Match = null;
            Match = personObj;
        }

        #endregion
    }
}
