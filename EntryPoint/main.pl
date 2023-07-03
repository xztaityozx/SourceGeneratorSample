use strict;
use warnings;

use lib "./lib/Generated/Perl";
use EntryPoint::SampleClass;


my $sample_class = EntryPoint::SampleClass->new(+{c => 100, d => 200});

print $sample_class->add(1,2);
