// <copyright file="CreatureStatConfiguration.cs" company="2Dudes">
// Copyright (c) 2018 2Dudes. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using COMMO.Data.Models;

namespace COMMO.Data.Configurations
{
    public class CreatureStatConfiguration : IEntityTypeConfiguration<CreatureStat>
    {
        public void Configure(EntityTypeBuilder<CreatureStat> builder)
        {
            builder.HasKey(b => new { b.Name, b.Time });

            builder.Property(t => t.KilledBy)
                .IsRequired();

            builder.Property(t => t.Killed)
                .IsRequired();
        }
    }
}
