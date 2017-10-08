﻿using Octoposh.Model;
using Octopus.Client.Model;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Octoposh.Infrastructure;

namespace Octoposh.Cmdlets
{
    /// <summary>
    /// <para type="synopsis"> Gets Octopus Variable sets. These can belong to a specific Project or to a Library Variable set. "Variable set" is the name of the object that holds the collection of variables for both Projects and Library Sets.</para>
    /// </summary>
    /// <summary>
    /// <para type="synopsis"> Gets Octopus Variable sets. These can belong to a specific Project or to a Library Variable set. "Variable set" is the name of the object that holds the collection of variables for both Projects and Library Sets.</para>
    /// </summary>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet</code>
    ///   <para>Gets all the Project and Library variable sets of the instance</para>    
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -LibrarySetName "Stands_SC"</code>
    ///   <para>Gets the Variable Set of the Library Variable Set with the name "Stands_SC"</para>    
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -LibrarySetName "Stands_SC" -IncludeUsage</code>
    ///   <para>Gets the Variable Set of the Library Variable Set "Stands_SC" and it also populates the output object property "Usage" with the list of projects that are currently using the set</para>    
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -LibrarySetName "Stands_SC","Stands_DII"</code>
    ///   <para>Gets the LibraryVariableSets with the names "Stands_SC" and "Stands_DII"</para>
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -LibrarySetName "Stands_*"</code>
    ///   <para>Gets all the LibraryVariableSets whose name matches the pattern "Stands_*"</para>    
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -LibrarySetName "Stands_*" -IncludeLibrarySetUsage </code>
    ///   <para>Gets all the LibraryVariableSets whose name matches the pattern "Stands_*". Each result will also include a list of Projects on which they are being used</para>
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -ProjectName "Website_Stardust","Website_Diamond"</code>
    ///   <para>Gets the Variable Sets of the Projects "Website_Stardust" and "Website_Diamond"</para>
    /// </example>
    /// <example>   
    ///   <code>PS C:\> Get-OctopusVariableSet -ProjectName "Website_Stardust" -LibrarySetName "Stands_SC"</code>
    ///   <para>Gets the Variable Sets of the Project "Website_Stardust" and the Library variable set "Stands_SC"</para>
    /// </example>
    /// <para type="link" uri="http://Octoposh.net">WebSite: </para>
    /// <para type="link" uri="https://github.com/Dalmirog/OctoPosh/">Github Project: </para>
    /// <para type="link" uri="http://octoposh.readthedocs.io">Wiki: </para>
    /// <para type="link" uri="https://gitter.im/Dalmirog/OctoPosh#initial">QA and Feature requests: </para>
    [Cmdlet("Get", "OctopusVariableSet")]
    [OutputType(typeof(List<OutputOctopusVariableSet>))]
    [OutputType(typeof(List<VariableSetResource>))]
    public class GetOctopusVariableSet : GetOctoposhCmdlet
    {
        /// <summary>
        /// <para type="description">Library Set name</para>
        /// </summary>
        [ValidateNotNullOrEmpty()]
        [Parameter(Position = 1, ValueFromPipeline = true)]
        public string[] LibrarySetName { get; set; }

        /// <summary>
        /// <para type="description">Project name</para>
        /// </summary>
        [ValidateNotNullOrEmpty()]
        [Parameter(Position = 1, ValueFromPipeline = true)]
        public string[] ProjectName { get; set; }

        /// <summary>
        /// <para type="description">If set to TRUE the list of Projects on which each Library Variable Set is being used will be displayer</para>
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeUsage { get; set; }

        protected override void ProcessRecord()
        {
            var variableSetIDs = new List<string>();

            var projectList = new List<ProjectResource>();
            var libraryVariableSetList = new List<LibraryVariableSetResource>();

            var librarySetNameList = LibrarySetName?.ToList().ConvertAll(s => s.ToLower());

            var projectNameList = ProjectName?.ToList().ConvertAll(s => s.ToLower());

            //If no Project and Library set is declared, return all the things.
            if (projectNameList == null && librarySetNameList == null)
            {
                projectList.AddRange(Connection.Repository.Projects.FindAll());
                libraryVariableSetList.AddRange(Connection.Repository.LibraryVariableSets.FindAll());
            }

            //If at least 1 project or Library variable set is defined, then just return that list instead of everything.
            else
            {
                //todo should revisit logic when user doesn't pass -IncludeUsage. Is it necesary to get all libraries and projects?
                //Getting projects to get their variable set IDs
                #region Getting projects
                if (projectNameList != null)
                {
                    projectList = FilterByName(projectNameList, Connection.Repository.Projects, "ProjectName");
                }
                #endregion
                
                #region Getting Library variable sets
                if (librarySetNameList != null)
                {
                    libraryVariableSetList = FilterByName(librarySetNameList, Connection.Repository.LibraryVariableSets,"LibrarySetName");
                }
                #endregion
            }

            variableSetIDs.AddRange(libraryVariableSetList.Select(v => v.VariableSetId));
            variableSetIDs.AddRange(projectList.Select(p => p.VariableSetId));

            //This doesn't work and throws: [Exception thrown: 'Octopus.Client.Exceptions.OctopusResourceNotFoundException' in Octopus.Client.dll]
            //Github issue for this https://github.com/OctopusDeploy/Issues/issues/3307
            //baseResourceList.AddRange(_connection.Repository.VariableSets.Get(variableSetIDs.ToArray()));

            //This works
            var baseResourceList = variableSetIDs.Select(id => Connection.Repository.VariableSets.Get(id)).ToList();

            if (ResourceOnly)
            {
                if (baseResourceList.Count == 1)
                {
                    WriteObject(baseResourceList.FirstOrDefault(),true);
                }
                else
                {
                    WriteObject(baseResourceList,true);
                }
            }

            else
            {
                var converter = new OutputConverter();
                var outputList = converter.GetOctopusVariableSet(baseResourceList,projectList,libraryVariableSetList,IncludeUsage);

                if (outputList.Count == 1)
                {
                    WriteObject(outputList.FirstOrDefault(),true);
                }
                else
                {
                    WriteObject(outputList,true);
                }
            }

        }
    }
}