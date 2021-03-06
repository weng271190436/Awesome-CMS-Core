using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AwesomeCMSCore.Modules.Admin.ViewModels;
using AwesomeCMSCore.Modules.Client.ViewModels;
using AwesomeCMSCore.Modules.Entities.Entities;
using AwesomeCMSCore.Modules.Entities.Enums;
using AwesomeCMSCore.Modules.Helper.Services;
using AwesomeCMSCore.Modules.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AwesomeCMSCore.Modules.Client.Repositories
{
	public class PostRepository: IPostRepository
	{
		private readonly IUserService _userService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly string _currentUserId;
		private readonly ILogger<PostRepository> _logger;

		public PostRepository(IUserService userService,
			IUnitOfWork unitOfWork,
			IMapper mapper,
			ILogger<PostRepository> logger)
		{
			_userService = userService;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_currentUserId = _userService.GetCurrentUserGuid();
			_logger = logger;
		}

		public async Task<IndexViewModel> GetIndexViewModel()
		{
			try
			{
				var postQuery = _unitOfWork.Repository<Post>().Query().Where(p => p.PostStatus.Equals(PostStatus.Published)).AsQueryable();
				var posts = await postQuery.ToListAsync();
				var popularPost = await postQuery.OrderByDescending(p => p.Views).Take(5).ToListAsync();
				var recentPost = await postQuery.OrderByDescending(p => p.DateCreated).FirstOrDefaultAsync();

				var categories = await _unitOfWork.Repository<PostOption>().Query()
					.Where(p => p.OptionType == PostOptionType.CategorieOptions)
					.Select(x => x.Key).FirstOrDefaultAsync();

				var vm = new IndexViewModel
				{
					Posts = _mapper.Map<IEnumerable<Post>, IEnumerable<PostListViewModel>>(posts),
					PopularPosts = _mapper.Map<IEnumerable<Post>, IEnumerable<PostListViewModel>>(popularPost),
					RecentPost = _mapper.Map<Post, PostIndexViewModel>(recentPost),
					Categories = categories
				};
				
				return vm;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message, ex);
				throw;
			}
		}
	}
}
