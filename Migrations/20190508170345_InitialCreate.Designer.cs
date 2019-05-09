﻿// <auto-generated />
using System;
using JBA.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace JBA.Migrations
{
    [DbContext(typeof(JBADbContext))]
    [Migration("20190508170345_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity("JBA.Model.PrecipitationRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<int>("Value");

                    b.Property<int>("Xref");

                    b.Property<int>("Yref");

                    b.HasKey("Id");

                    b.ToTable("PrecipitationRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
