﻿// <auto-generated />
using System;
using CategoryMicroservice.DBContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CategoryMicroservice.Migrations
{
    [DbContext(typeof(CategoryDbContext))]
    [Migration("20240828201119_CreateInit")]
    partial class CreateInit
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CategoryMicroservice.DBContexts.Entities.Category", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("CategoryMicroservice.DBContexts.Entities.CategoryDetail", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CategoryId", "ProductId");

                    b.ToTable("CategoryDetails");
                });

            modelBuilder.Entity("CategoryMicroservice.DBContexts.Entities.CategoryDetail", b =>
                {
                    b.HasOne("CategoryMicroservice.DBContexts.Entities.Category", "Category")
                        .WithMany("CategoryDetails")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("CategoryMicroservice.DBContexts.Entities.Category", b =>
                {
                    b.Navigation("CategoryDetails");
                });
#pragma warning restore 612, 618
        }
    }
}
