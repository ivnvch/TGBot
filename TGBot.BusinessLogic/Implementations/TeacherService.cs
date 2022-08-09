using AutoMapper;
using TGBot.BusinessLogic.Interfaces;
using TGBot.Common.Mapper;
using Microsoft.EntityFrameworkCore;
using TGBot.Models;

namespace TGBot.BusinessLogic.Implementations
{
    public class TeacherService:ITeacherService
    {
        public IMapper Mapper { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IMapper _mapper;
        DataContext _context;

        public TeacherService(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public TeacherDTO Get(int id)
        {
            return _mapper.Map<TeacherDTO>(_context.Sections.AsNoTracking().FirstOrDefault(x => x.Id == id));
        }

        public IEnumerable<TeacherDTO> Gets()
        {
            return _mapper.Map<List<TeacherDTO>>(_context.Sections.AsNoTracking().ToList());
        }
    }
}
