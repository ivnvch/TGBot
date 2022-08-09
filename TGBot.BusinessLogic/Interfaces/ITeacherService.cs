﻿using TGBot.Common.Mapper;

namespace TGBot.BusinessLogic.Interfaces
{
    public interface ITeacherService
    {
        IEnumerable<TeacherDTO> Gets();
        TeacherDTO Get(int id);
        //IMapper Mapper { get; set; }
    }
}
