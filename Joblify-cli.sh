#!/bin/bash

# ============================================================
#  Joblify CLI
#  Usage:
#    ./joblify-cli.sh generate module <module-name>
#    ./joblify-cli.sh generate <component-type> <n> <module-name>
#    ./joblify-cli.sh list
#    ./joblify-cli.sh --help
# ============================================================

# ── Colors ────────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
WHITE='\033[1;37m'
RED='\033[0;31m'
BLUE='\033[0;34m'
BOLD='\033[1m'
NC='\033[0m'

# ── Config ────────────────────────────────────────────────────────────────────
NAMESPACE="Joblify"
CREATED=0
SKIPPED=0

# ─────────────────────────────────────────────────────────────────────────────
#  PRINT HELPERS
# ─────────────────────────────────────────────────────────────────────────────
print_header() {
  echo ""
  echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo -e "${CYAN}  $1${NC}"
  echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  echo ""
}

print_success() { echo -e "  ${GREEN}[OK]   $1${NC}"; }
print_error()   { echo -e "  ${RED}[ERR]  $1${NC}"; }
print_warning() { echo -e "  ${YELLOW}[SKIP] $1${NC}"; }
print_info()    { echo -e "  ${BLUE}[INFO] $1${NC}"; }

print_banner() {
  echo ""
  echo -e "${CYAN}╔══════════════════════════════════════════════════════╗${NC}"
  echo -e "${CYAN}║           Joblify CLI  —  Module Generator           ║${NC}"
  echo -e "${CYAN}╚══════════════════════════════════════════════════════╝${NC}"
  echo ""
}

# ─────────────────────────────────────────────────────────────────────────────
#  STRING UTILITIES
# ─────────────────────────────────────────────────────────────────────────────

# kebab-case or snake_case  →  PascalCase
# e.g.  job-application  →  JobApplication
kebab_to_pascal() {
  local input="$1"
  echo "$input" \
    | sed 's/[-_]/ /g' \
    | awk '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) substr($i,2); print}' \
    | sed 's/ //g'
}

# Naive pluralise — handles common English patterns
# e.g.  Job → Jobs   Company → Companies   Status → Status
pluralize() {
  local word="$1"
  case "$word" in
    *status|*Status) echo "$word" ;;   # already uncountable
    *s)              echo "$word" ;;   # already looks plural
    *y)              echo "${word%y}ies" ;;  # Company → Companies
    *)               echo "${word}s"   ;;
  esac
}

# PascalCase → lowercase  (used for route names)
to_lower() {
  echo "$1" | tr '[:upper:]' '[:lower:]'
}

# ─────────────────────────────────────────────────────────────────────────────
#  FILE HELPERS
# ─────────────────────────────────────────────────────────────────────────────
create_dir() {
  mkdir -p "$1"
}

write_file() {
  local path="$1"
  local content="$2"
  create_dir "$(dirname "$path")"

  if [ -f "$path" ]; then
    print_warning "Already exists — skipped: $path"
    SKIPPED=$((SKIPPED + 1))
  else
    printf '%s\n' "$content" > "$path"
    print_success "Created: $path"
    CREATED=$((CREATED + 1))
  fi
}

# ask_yes_no <question> <default>   →  returns 0=yes  1=no
ask_yes_no() {
  local question="$1"
  local default="${2:-y}"
  local prompt
  [ "$default" = "y" ] && prompt="Y/n" || prompt="y/N"

  while true; do
    echo -ne "  ${WHITE}$question ${GRAY}[$prompt]: ${NC}"
    read -r answer
    answer="${answer:-$default}"
    case "$answer" in
      [Yy]*) return 0 ;;
      [Nn]*) return 1 ;;
      *)     print_error "Please enter y or n." ;;
    esac
  done
}

# ─────────────────────────────────────────────────────────────────────────────
#  COMPONENT CREATORS
#  $1 = module_name  (plural PascalCase folder)  e.g. Jobs
#  $2 = service_name (singular PascalCase)        e.g. Job
# ─────────────────────────────────────────────────────────────────────────────

create_enum() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/Enums/${service_name}Status.cs" \
"namespace $NAMESPACE.Modules.$module_name.Enums;

public enum ${service_name}Status
{
    Active = 1,
    Inactive = 2,
    Pending = 3,
    Deleted = 4
}"
}

create_entity() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/Entities/${service_name}.cs" \
"using $NAMESPACE.Modules.$module_name.Enums;

namespace $NAMESPACE.Modules.$module_name.Entities;

public class $service_name
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // TODO: Add ${service_name}-specific properties here
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ${service_name}Status Status { get; set; } = ${service_name}Status.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}"
}

create_configuration() {
  local module_name="$1" service_name="$2"
  local table_name
  table_name=$(to_lower "$service_name")
  write_file "Modules/$module_name/Configurations/${service_name}Configuration.cs" \
"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using $NAMESPACE.Modules.$module_name.Entities;

namespace $NAMESPACE.Modules.$module_name.Configurations;

public class ${service_name}Configuration : IEntityTypeConfiguration<$service_name>
{
    public void Configure(EntityTypeBuilder<$service_name> builder)
    {
        builder.ToTable(\"${table_name}s\");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasDefaultValueSql(\"gen_random_uuid()\");

        builder.Property(x => x.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Description)
               .HasMaxLength(2000);

        builder.Property(x => x.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql(\"now()\");

        builder.Property(x => x.UpdatedAt)
               .IsRequired(false);

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);

        // TODO: Add relationships, additional indexes, or constraints here
    }
}"
}

create_dto() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/DTOs/${service_name}Dto.cs" \
"using System.ComponentModel.DataAnnotations;
using $NAMESPACE.Modules.$module_name.Enums;

namespace $NAMESPACE.Modules.$module_name.DTOs;

// ── Response DTO ──────────────────────────────────────────────────────────────
public class ${service_name}Dto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── Create DTO ────────────────────────────────────────────────────────────────
public class Create${service_name}Dto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
}

// ── Update DTO ────────────────────────────────────────────────────────────────
public class Update${service_name}Dto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public ${service_name}Status? Status { get; set; }
}"
}

create_repository_interface() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/Interfaces/I${service_name}Repository.cs" \
"using $NAMESPACE.Modules.$module_name.Entities;

namespace $NAMESPACE.Modules.$module_name.Interfaces;

public interface I${service_name}Repository
{
    Task<IEnumerable<$service_name>> GetAllAsync();
    Task<$service_name?> GetByIdAsync(Guid id);
    Task AddAsync($service_name entity);
    void Update($service_name entity);
    void Delete($service_name entity);
    Task<bool> SaveChangesAsync();
}"
}

create_repository() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/Repositories/${service_name}Repository.cs" \
"using Microsoft.EntityFrameworkCore;
using $NAMESPACE.Data;
using $NAMESPACE.Modules.$module_name.Entities;
using $NAMESPACE.Modules.$module_name.Interfaces;

namespace $NAMESPACE.Modules.$module_name.Repositories;

public class ${service_name}Repository : I${service_name}Repository
{
    private readonly AppDbContext _context;

    public ${service_name}Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<$service_name>> GetAllAsync()
        => await _context.$module_name
                         .AsNoTracking()
                         .OrderByDescending(x => x.CreatedAt)
                         .ToListAsync();

    public async Task<$service_name?> GetByIdAsync(Guid id)
        => await _context.$module_name
                         .AsNoTracking()
                         .FirstOrDefaultAsync(x => x.Id == id);

    public async Task AddAsync($service_name entity)
        => await _context.$module_name.AddAsync(entity);

    public void Update($service_name entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.$module_name.Update(entity);
    }

    public void Delete($service_name entity)
        => _context.$module_name.Remove(entity);

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}"
}

create_service() {
  local module_name="$1" service_name="$2"

  write_file "Modules/$module_name/Interfaces/I${service_name}Service.cs" \
"using $NAMESPACE.Modules.$module_name.DTOs;

namespace $NAMESPACE.Modules.$module_name.Interfaces;

public interface I${service_name}Service
{
    Task<IEnumerable<${service_name}Dto>> GetAllAsync();
    Task<${service_name}Dto?> GetByIdAsync(Guid id);
    Task<${service_name}Dto> CreateAsync(Create${service_name}Dto dto);
    Task<${service_name}Dto?> UpdateAsync(Guid id, Update${service_name}Dto dto);
    Task<bool> DeleteAsync(Guid id);
}"

  write_file "Modules/$module_name/Services/${service_name}Service.cs" \
"using $NAMESPACE.Modules.$module_name.DTOs;
using $NAMESPACE.Modules.$module_name.Entities;
using $NAMESPACE.Modules.$module_name.Interfaces;

namespace $NAMESPACE.Modules.$module_name.Services;

public class ${service_name}Service : I${service_name}Service
{
    private readonly I${service_name}Repository _repository;

    public ${service_name}Service(I${service_name}Repository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<${service_name}Dto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto);
    }

    public async Task<${service_name}Dto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<${service_name}Dto> CreateAsync(Create${service_name}Dto dto)
    {
        var entity = new $service_name
        {
            Title       = dto.Title,
            Description = dto.Description
        };

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<${service_name}Dto?> UpdateAsync(Guid id, Update${service_name}Dto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        if (dto.Title is not null)       entity.Title       = dto.Title;
        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.Status is not null)      entity.Status      = dto.Status.Value;

        _repository.Update(entity);
        await _repository.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return false;

        _repository.Delete(entity);
        return await _repository.SaveChangesAsync();
    }

    // ── Manual Mapping ────────────────────────────────────────────────────────
    private static ${service_name}Dto MapToDto($service_name entity) => new()
    {
        Id          = entity.Id,
        Title       = entity.Title,
        Description = entity.Description,
        Status      = entity.Status.ToString(),
        CreatedAt   = entity.CreatedAt,
        UpdatedAt   = entity.UpdatedAt
    };
}"
}

create_controller() {
  local module_name="$1" service_name="$2"
  local route
  route=$(to_lower "$module_name")
  write_file "Modules/$module_name/Controllers/${service_name}Controller.cs" \
"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using $NAMESPACE.Modules.$module_name.DTOs;
using $NAMESPACE.Modules.$module_name.Interfaces;

namespace $NAMESPACE.Modules.$module_name.Controllers;

[ApiController]
[Route(\"api/$route\")]
[Authorize]
public class ${service_name}Controller : ControllerBase
{
    private readonly I${service_name}Service _service;

    public ${service_name}Controller(I${service_name}Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet(\"{id:guid}\")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create${service_name}Dto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut(\"{id:guid}\")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Update${service_name}Dto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete(\"{id:guid}\")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}"
}

create_module_file() {
  local module_name="$1" service_name="$2"
  write_file "Modules/$module_name/${service_name}Module.cs" \
"using Microsoft.Extensions.DependencyInjection;
using $NAMESPACE.Modules.$module_name.Repositories;
using $NAMESPACE.Modules.$module_name.Services;

namespace $NAMESPACE.Modules.$module_name;

public static class ${service_name}Module
{
    public static IServiceCollection Add${service_name}Module(this IServiceCollection services)
    {
        services.AddScoped<I${service_name}Repository, ${service_name}Repository>();
        services.AddScoped<I${service_name}Service, ${service_name}Service>();

        return services;
    }
}"
}

# ─────────────────────────────────────────────────────────────────────────────
#  GENERATE MODULE
# ─────────────────────────────────────────────────────────────────────────────
generate_module() {
  local module_input="$1"

  if [[ -z "$module_input" ]]; then
    print_error "Module name is required"
    echo "  Usage: ./joblify-cli.sh generate module <module-name>"
    exit 1
  fi

  local base_name
  base_name=$(kebab_to_pascal "$module_input")
  local module_name
  module_name=$(pluralize "$base_name")  # folder  e.g. Jobs
  local service_name="$base_name"        # classes e.g. Job
  local module_dir="Modules/$module_name"

  print_banner
  print_header "Generating module: $module_name"

  if [[ -d "$module_dir" ]]; then
    print_error "Module '$module_name' already exists at $module_dir"
    exit 1
  fi

  create_dir "$module_dir"

  # ── Always created ─────────────────────────────────────────────────────────
  echo -e "  ${BOLD}── Core Architecture (always generated) ─────────────${NC}"
  create_repository_interface "$module_name" "$service_name"
  create_repository           "$module_name" "$service_name"
  create_service          "$module_name" "$service_name"
  echo ""

  # ── Optional ───────────────────────────────────────────────────────────────
  echo -e "  ${BOLD}── Implementation Details (optional) ───────────────${NC}"
  ask_yes_no "  Generate enum?" "y"                 && create_enum          "$module_name" "$service_name"
  ask_yes_no "  Generate entity?" "y"                && create_entity        "$module_name" "$service_name"
  ask_yes_no "  Generate entity configuration?" "y"  && create_configuration "$module_name" "$service_name"
  ask_yes_no "  Generate DTOs?" "y"                  && create_dto           "$module_name" "$service_name"
  ask_yes_no "  Generate controller?" "y"            && create_controller    "$module_name" "$service_name"
  echo ""

  # ── Always created ─────────────────────────────────────────────────────────
  echo -e "  ${BOLD}── DI Registration (always generated) ───────────────${NC}"
  create_module_file "$module_name" "$service_name"

  # ── Summary ───────────────────────────────────────────────────────────────
  echo ""
  echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
  print_success "Module '$module_name' generated successfully!"
  echo -e "  ${GREEN}  Files created : $CREATED${NC}"
  echo -e "  ${YELLOW}  Files skipped : $SKIPPED${NC}"
  echo ""
  print_info "Next steps:"
  echo ""
  echo "    1. Add DbSet to AppDbContext.cs:"
  echo -e "       ${YELLOW}public DbSet<$service_name> $module_name => Set<$service_name>();${NC}"
  echo ""
  echo "    2. Register EF config in OnModelCreating:"
  echo -e "       ${YELLOW}modelBuilder.ApplyConfiguration(new ${service_name}Configuration());${NC}"
  echo ""
  echo "    3. Register module in Program.cs:"
  echo -e "       ${YELLOW}builder.Services.Add${service_name}Module();${NC}"
  echo ""
  echo "    4. Run migration:"
  echo -e "       ${YELLOW}dotnet ef migrations add Add${module_name}${NC}"
  echo -e "       ${YELLOW}dotnet ef database update${NC}"
  echo ""
}

# ─────────────────────────────────────────────────────────────────────────────
#  GENERATE SINGLE COMPONENT into existing module
# ─────────────────────────────────────────────────────────────────────────────
generate_component() {
  local component_type="$1"
  local component_input="$2"
  local module_input="$3"

  if [[ -z "$component_input" || -z "$module_input" ]]; then
    print_error "Component name and module name are required"
    echo "  Usage: ./joblify-cli.sh generate <component-type> <name> <module-name>"
    exit 1
  fi

  local pascal_component
  pascal_component=$(kebab_to_pascal "$component_input")
  local pascal_base
  pascal_base=$(kebab_to_pascal "$module_input")
  local pascal_module
  pascal_module=$(pluralize "$pascal_base")

  if [[ ! -d "Modules/$pascal_module" ]]; then
    print_error "Module '$pascal_module' does not exist. Create it first:"
    echo "  ./joblify-cli.sh generate module $module_input"
    exit 1
  fi

  print_header "Adding $component_type → $pascal_module"

  case "$component_type" in
    "enum")
      create_enum                 "$pascal_module" "$pascal_component" ;;
    "entity")
      create_entity               "$pascal_module" "$pascal_component" ;;
    "configuration"|"config")
      create_configuration        "$pascal_module" "$pascal_component" ;;
    "dto")
      create_dto                  "$pascal_module" "$pascal_component" ;;
    "repository-interface"|"repo-interface"|"interface")
      create_repository_interface "$pascal_module" "$pascal_component" ;;
    "repository"|"repo")
      create_repository           "$pascal_module" "$pascal_component" ;;
    "service")
      create_service              "$pascal_module" "$pascal_component" ;;
    "controller")
      create_controller           "$pascal_module" "$pascal_component" ;;
    *)
      print_error "Unknown component type: '$component_type'"
      echo ""
      echo "  Available types:"
      echo "    enum, entity, configuration, dto"
      echo "    repository-interface, repository, service, controller"
      exit 1
      ;;
  esac

  echo ""
  print_success "Done."
  echo ""
}

# ─────────────────────────────────────────────────────────────────────────────
#  LIST MODULES
# ─────────────────────────────────────────────────────────────────────────────
list_modules() {
  print_header "Available modules"

  if [[ ! -d "Modules" ]]; then
    print_warning "No Modules directory found in current folder."
    return
  fi

  local modules
  modules=$(find Modules -maxdepth 1 -mindepth 1 -type d | sort)

  if [[ -z "$modules" ]]; then
    print_warning "No modules found."
    return
  fi

  for module in $modules; do
    local module_name
    module_name=$(basename "$module")
    local components=()

    [[ -d "$module/Enums" ]]          && components+=("Enums")
    [[ -d "$module/Entities" ]]       && components+=("Entities")
    [[ -d "$module/Configurations" ]] && components+=("Configurations")
    [[ -d "$module/DTOs" ]]           && components+=("DTOs")
    [[ -d "$module/Interfaces" ]]     && components+=("Interfaces")
    [[ -d "$module/Repositories" ]]   && components+=("Repositories")
    [[ -d "$module/Services" ]]       && components+=("Services")
    [[ -d "$module/Controllers" ]]    && components+=("Controllers")

    if [[ ${#components[@]} -gt 0 ]]; then
      print_info "$module_name"
      echo -e "       ${GRAY}└── $(IFS=', '; echo "${components[*]}")${NC}"
    else
      print_warning "$module_name (empty)"
    fi
  done

  echo ""
}

# ─────────────────────────────────────────────────────────────────────────────
#  HELP
# ─────────────────────────────────────────────────────────────────────────────
show_help() {
  print_banner
  echo -e "${WHITE}  USAGE${NC}"
  echo ""
  echo -e "  ${YELLOW}./joblify-cli.sh generate module <name>${NC}"
  echo -e "  ${GRAY}    Scaffold a full module interactively${NC}"
  echo ""
  echo -e "  ${YELLOW}./joblify-cli.sh generate <component> <name> <module>${NC}"
  echo -e "  ${GRAY}    Add a single component to an existing module${NC}"
  echo ""
  echo -e "  ${YELLOW}./joblify-cli.sh list${NC}"
  echo -e "  ${GRAY}    List all existing modules and their components${NC}"
  echo ""
  echo -e "${WHITE}  COMPONENT TYPES${NC}"
  echo ""
  echo -e "  ${CYAN}enum${NC}                  Enums/<n>Status.cs"
  echo -e "  ${CYAN}entity${NC}                Entities/<n>.cs"
  echo -e "  ${CYAN}configuration${NC}         Configurations/<n>Configuration.cs"
  echo -e "  ${CYAN}dto${NC}                   DTOs/<n>Dto.cs"
  echo -e "  ${CYAN}repository-interface${NC}  Interfaces/I<n>Repository.cs"
  echo -e "  ${CYAN}repository${NC}            Repositories/<n>Repository.cs"
  echo -e "  ${CYAN}service${NC}               Interfaces/I<n>Service.cs + Services/<n>Service.cs"
  echo -e "  ${CYAN}controller${NC}            Controllers/<n>Controller.cs"
  echo ""
  echo -e "${WHITE}  EXAMPLES${NC}"
  echo ""
  echo -e "  ${GRAY}# Full module (interactive prompts)${NC}"
  echo -e "  ${YELLOW}./joblify-cli.sh generate module job${NC}"
  echo ""
  echo -e "  ${GRAY}# Kebab-case input auto-converts to PascalCase${NC}"
  echo -e "  ${YELLOW}./joblify-cli.sh generate module job-application${NC}"
  echo ""
  echo -e "  ${GRAY}# Add a DTO to an existing Jobs module${NC}"
  echo -e "  ${YELLOW}./joblify-cli.sh generate dto JobFilter job${NC}"
  echo ""
  echo -e "  ${GRAY}# Add a controller to an existing Jobs module${NC}"
  echo -e "  ${YELLOW}./joblify-cli.sh generate controller Job job${NC}"
  echo ""
  echo -e "  ${GRAY}# List all modules${NC}"
  echo -e "  ${YELLOW}./joblify-cli.sh list${NC}"
  echo ""
}

# ─────────────────────────────────────────────────────────────────────────────
#  ENTRY POINT
# ─────────────────────────────────────────────────────────────────────────────
COMMAND="$1"
SUB_COMMAND="$2"
ARG_ONE="$3"
ARG_TWO="$4"

case "$COMMAND" in
  generate | g)
    case "$SUB_COMMAND" in
      module | m)
        generate_module "$ARG_ONE"
        ;;
      enum | entity | configuration | config | dto \
      | repository-interface | repo-interface | interface \
      | repository | repo | service | controller)
        generate_component "$SUB_COMMAND" "$ARG_ONE" "$ARG_TWO"
        ;;
      "")
        print_error "Sub-command is required."
        echo "  Try: ./joblify-cli.sh --help"
        exit 1
        ;;
      *)
        print_error "Unknown sub-command: '$SUB_COMMAND'"
        echo "  Try: ./joblify-cli.sh --help"
        exit 1
        ;;
    esac
    ;;

  list | ls)
    list_modules
    ;;

  --help | -h | help | "")
    show_help
    ;;

  *)
    print_error "Unknown command: '$COMMAND'"
    show_help
    exit 1
    ;;
esac