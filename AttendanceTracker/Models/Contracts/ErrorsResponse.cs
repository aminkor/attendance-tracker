﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;

namespace TMLink_BackEnd.Models.Contracts
{
    public class ErrorsResponse
    {
        List<ErrorMessage> _errors;

        public ErrorsResponse()
        {
            _errors = new List<ErrorMessage>();
        }

        public ErrorsResponse(List<ErrorMessage> errors)
        {
            _errors = errors;
        }

        public void Add(ErrorMessage error)
        {
            if (!_errors.Contains(error))
                _errors.Add(error);
        }

        public IEnumerable<ErrorMessage> Errors
        {
            get
            {
                foreach (var error in _errors)
                    yield return error;
            }
        }
    }

    public class ErrorMessage
    {
        public String Message { get; set; }
        public int Code { get; set; }
    }


}
