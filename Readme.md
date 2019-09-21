
# Introduction

This is a small tool to format a table in Markdown conveniently.

# Quick Start

Before starting with this section, please find mtf.exe and demo1.md in the project. mtf.exe is compiled by the project.

## Sort by the first column

1. Create a table.

The table header needs to be defined. The separater between the header and the 1st row can be imcomplete, but should be put something. Then you can put rows in any order, which will be sorted by the tool by the 1st column in alphabetical order.

```markdown
| SYMBOL | COMPANY | PRICE (INTRADAY) | CHANGE | %CHANGE |
|---
| T | AT&T Inc. | $37.91 | +0.76 | 2.05% |
| HD | Home Depot, Inc. (The) Common Stock | $224.67 | -3.45 | 1.51% |
| VZ | Verizon Communications Inc. Common Stock | $60.29 | +0.31 | 0.52% |
| DIS | Walt Disney Company (The) Common Stock | $132.27 | -1.03 | 0.77% |
```

2. Run the tool

Run the tool with the file name of the markdonw file. The file is updated with the formatted table.

```cmd
cmd> mtf demo1.md
```

Here is the result.

```markdown
| SYMBOL | COMPANY | PRICE (INTRADAY) | CHANGE | %CHANGE |
|--------|---------|------------------|--------|---------|
| DIS | Walt Disney Company (The) Common Stock | $132.27 | -1.03 | 0.77% |
| HD | Home Depot, Inc. (The) Common Stock | $224.67 | -3.45 | 1.51% |
| T | AT&T Inc. | $37.91 | +0.76 | 2.05% |
| VZ | Verizon Communications Inc. Common Stock | $60.29 | +0.31 | 0.52% |
```

## Adjust the column width

1. Run the tool with --padding option

If you run the tool with --padding option, the column width are adjusted as well.

```cmd
cmd> mtf demo1.md --padding
```

Here is the result.

```markdown
| SYMBOL | COMPANY | PRICE (INTRADAY) | CHANGE | %CHANGE |
|--------|---------|------------------|--------|---------|
| DIS | Walt Disney Company (The) Common Stock   | $132.27 | -1.03 | 0.77% |
| HD  | Home Depot, Inc. (The) Common Stock      | $224.67 | -3.45 | 1.51% |
| T   | AT&T Inc.                                | $37.91  | +0.76 | 2.05% |
| VZ  | Verizon Communications Inc. Common Stock | $60.29  | +0.31 | 0.52% |
```

# Dependencies

This project depends on the following packages. Install them to compile the project.

* [CommandLineParser](https://www.nuget.org/packages/CommandLineParser)
  
  
  ```nuget
  PM> Install-Package CommandLineParser -Version 2.6.0
  ```

