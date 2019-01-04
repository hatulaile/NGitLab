﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGitLab.Models;

namespace NGitLab.Impl
{
    public class PipelineClient : IPipelineClient
    {
        private readonly API _api;
        private readonly string _projectPath;
        private readonly string _pipelinesPath;

        public PipelineClient(API api, int projectId)
        {
            _api = api;
            _projectPath = $"{Project.Url}/{projectId}";
            _pipelinesPath = $"{Project.Url}/{projectId}/pipelines";
        }

        public IEnumerable<PipelineBasic> All => _api.Get().GetAll<PipelineBasic>(_pipelinesPath);

        public IEnumerable<Job> AllJobs => _api.Get().GetAll<Job>($"{_projectPath}/jobs");

        [Obsolete("Use JobClient.GetJobs() instead")]
        public IEnumerable<Job> GetJobsInProject(JobScope scope)
        {
            var url = $"{_projectPath}/jobs";

            if (scope != JobScope.All)
            {
                url = Utils.AddParameter(url, "scope", scope.ToString().ToLowerInvariant());
            }

            return _api.Get().GetAll<Job>(url);
        }

        public Pipeline this[int id] => _api.Get().To<Pipeline>($"{_pipelinesPath}/{id}");

        public Job[] GetJobs(int pipelineId)
        {
            // For a reason gitlab returns the jobs in the reverse order as
            // they appear in their UI. Here we reverse them!

            var jobs = _api.Get().GetAll<Job>($"{_pipelinesPath}/{pipelineId}/jobs").Reverse().ToArray();
            return jobs;
        }

        public Pipeline Create(string @ref)
        {
            return _api.Post().To<Pipeline>($"{_projectPath}/pipeline?ref={@ref}");
        }

        public Pipeline CreatePipelineWithTrigger(string token, string @ref, Dictionary<string, string> variables)
        {
            var variablesToAdd = new StringBuilder();
            foreach (var variable in variables)
            {
                variablesToAdd.Append("&variables[").Append(variable.Key).Append("]=").Append(variable.Value);
            }

            return _api.Post().To<Pipeline>($"{_projectPath}/trigger/pipeline?token={token}&ref={@ref}{variablesToAdd}");
        }
    }
}
