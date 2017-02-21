#!/usr/local/bin/perl5
use Data::Dumper;
use Time::Local;
use POSIX;
use env_unix;
use DBI;
use CGI;
use Date::Calc qw(Mktime);
use JSON;
use lib "/exec/apps/bin/lib/perl5/";
require "/exec/apps/bin/lib/perl5/dbconn.pm";
print "Content-type: application/json\n\n";
my $cgi=new CGI;
my $date=$cgi->param('time');
#$date='2016-12-31';
exit if ($date eq "");
{
	my $c_time=time();
	our $chart_time=24;
#print strftime('%F %T',localtime($c_time));
	my @date=split('-',$date);
	our $s_time=&Mktime($date[0],$date[1],$date[2],'08','00','00');
#print strftime('%F %T',localtime($s_time));
	our $e_time=$s_time+60*60*$chart_time;
	$e_time=$c_time if($c_time<$e_time); 
#print strftime('%F %T',localtime($e_time));
#print "$s_time $e_time";
}
sub query_mode{
	my %unit;
	my $dbh=DBI->connect(&getconn('tjn','probeweb','readwrite')) or die "{\"result\":\"$!\"}";
	my $sql=qq{select host,c_mode,part,lot,wafer,pass,c_scope,usr,c_time,l_mode,l_info from daemon_lock where c_time>=$s_time and c_time<=$e_time  order by c_time};
#	my $sql=qq{select host,c_mode,part,lot,wafer,pass,c_scope,usr,c_time,l_mode,l_info from daemon_lock where c_time>=$s_time and c_time<=$e_time  and host='b3j75050' order by c_time};
	$sth = $dbh->prepare($sql) or die $!;
	$sth->execute() or die $!;
	$sth->bind_columns(undef,\$host,\$c_mode,\$part,\$lot,\$wafer,\$pass,\$c_scope,\$usr,\$c_time,\$l_mode,\$l_info);
        while ( $sth->fetch() ) {
		my $c_info="$usr,$c_scope,$part,$lot,$wafer,$pass";
		if ($c_mode eq $l_mode){
#skip the repeat column
			if (defined  $unit{$host}){
				next if ($c_info eq $l_info);	
			}
			next if ($c_mode eq "PRO");
		}
		my %info;
#current mode
		$info{c_mode}=$c_mode;
#last mode
		$info{l_mode}=$l_mode;
		$info{'time'}=$c_time;
#last info
		$info{l_info}=$l_info;
#current info
		$info{c_info}=$c_info;
		if (defined $unit{$host}){
			my $ref=$unit{$host};	
			push @$ref,\%info;
		}else{
			my @data=(\%info);
			$unit{$host}=\@data;
		}
        }
	$dbh->disconnect(); 
	return \%unit;
}
sub cal_mode{
	#initiall series_data 0
	my $xAxis_ref=shift;
	my $unit_ref=shift;
	my @xAxis=@$xAxis_ref;
	my %unit=%$unit_ref;
	my @series_data;
	foreach (@xAxis){
		push @series_data,0;
	}
	#end
	my @series;
	my $index=0;
	foreach my $host(@xAxis){
		my $ref=$unit{$host};
		my @data=@$ref;
		my $last_time;
#begin with the last info
		for(my $i=0;$i<=@data;$i++){
			$last_time=$s_time if($i==0);
			my %info;
			my @current_series_data=@series_data;
			if ($i!=@data){
				my $data_ref=$data[$i];
				my %data=%$data_ref;
				if ($data{l_mode} eq "PRO"){
					$info{color}='#00FF00';
				}elsif($data{l_mode} eq "ENG"){
					$info{color}='#0000E3';
				}
				$info{name}=$data{l_info};
				$current_series_data[$index]=($data{'time'}-$last_time)/86400*$chart_time;
				$info{data}=\@current_series_data;
				$info{stack}='mode';
	#		print $current_series_data[$index]."\n";
	#		print Dumper(\%info);
				push @series,\%info;
				$last_time=$data{'time'};
			}else{
					my $data_ref=$data[$i-1];
					my %data=%$data_ref;
					if ($data{c_mode} eq "PRO"){
							$info{color}='#00FF00';
					}elsif($data{c_mode} eq "ENG"){
							$info{color}='#0000E3';
					}
					$info{name}=$data{c_info};
					$current_series_data[$index]=($e_time-$data{'time'})/86400*$chart_time;
					$info{data}=\@current_series_data;
					$info{stack}='mode';
#			print $current_series_data[$index]."\n";
                	push @series,\%info;
#			print Dumper(\@series);
#			print Dumper(\%info);
			}
		}
		$index+=1;
	}
	return @series;
}
sub cal_EMS{
        #initiall series_data 0
	my %color=(
		"RUNNING"=>"#00FF00",
		"QUAL"=>"#FFFF37",
		"UALARM"=>'#9932CC', 
		"IDLE"=>'#FFFFFF',
		"EXPPROC"=>'#0000C6',
		"NOSUPP"=>'#FF1493',
		"SMMAINT"=>'#FFA500',
		"UMMAINT"=>'#FF0000',
		"RUNCOND"=>'#3CB370',
		"UENGEVL"=>'#00FFFF',
		'UENDATA'=>'#7A68EF',
		'OFFLINE'=>'#8B4412',
		'NOPROD'=>'#FFC1CB',
		'SETUP'=>'#FF0000',
		'EQUPGRD'=>'#6495ED',
	);
#	print Dumper(\%color);
        my $xAxis_ref=shift;
        my $unit_ref=shift;
        my @xAxis=@$xAxis_ref;
        my %unit=%$unit_ref;
        my @series_data;
        foreach (@xAxis){
                push @series_data,0;
        }
        #end
        my @ems_series;
        my $index=0;
        foreach my $host(@xAxis){
                my $ref=$unit{$host};
		#if EMS status is null
		if ($ref eq ''){
			my %info;
			my @current_series_data=@series_data;
			$current_series_data[$index]=($e_time-$s_time)/86400*$chart_time;
			$info{data}=\@current_series_data;
			$info{color}='#6C6C6C';
			$info{name}='UNKNOWN';
			$info{stack}='ems_mode';
			push @ems_series,\%info;
			$index+=1;
			next;
		}
                my @data=@$ref;
                my $last_time;
                for(my $i=0;$i<=@data;$i++){
                        $last_time=$s_time if($i==0);
                                        my %info;
                                        my @current_series_data=@series_data;
                        if ($i!=@data){
                                                my $data_ref=$data[$i];
                                                my %data=%$data_ref;
				$status=$data{l_status};
				$info{color}=$color{$status};
				$info{color}='#FF0000' if ($info{color} eq "");
                                $info{name}=$status;
                                $current_series_data[$index]=($data{'time'}-$last_time)/86400*$chart_time;
                                $info{data}=\@current_series_data;
                                $info{stack}='ems_mode';
        #               print $current_series_data[$index]."\n";
        #               print Dumper(\%info);
                                push @ems_series,\%info;
                                $last_time=$data{'time'};
                                }else{
                                        my $data_ref=$data[$i-1];
                                        my %data=%$data_ref;
                                	$status=$data{c_status};
                                	$info{color}=$color{$status};
					$info{color}='#FF0000' if ($info{color} eq "");
                                        $info{name}=$status;
                                        $current_series_data[$index]=($e_time-$data{'time'})/86400*$chart_time;
                                        $info{data}=\@current_series_data;
                                        $info{stack}='ems_mode';
#                       print $current_series_data[$index]."\n";
                        push @ems_series,\%info;
#                       print Dumper(\@series);
#                       print Dumper(\%info);
                        }
                }
                $index+=1;
        }
        return @ems_series;
}
sub EQPIDtrans{
	my $opt=shift;
	my $EQPID=shift;

	if ($opt eq "2EMS"){
		if ($EQPID=~/b3(\w+)(\d{2})/){
				my $type=$1;
				my $id=$2;
		$type='750' if ($type=~/j750/);
		$type='FLX' if ($type=~/flex/);
		$EQPID="T$id"."S$type";
		}		
	}elsif($opt eq "2host"){
		if ($EQPID=~/T(\d{2})S(\w+)/){
				my $id=$1;
				my $type=$2;
		$type='b3j750' if ($type=~/750/);
		$type='b3flex' if ($type=~/FLX/);
		$EQPID="$type$id";
		}
	}
	return $EQPID;
}
sub query_EMS{	
	my $xAxis_ref=shift;
	my %unit;
	my @xAxis=@$xAxis_ref;
	return \%unit if (@xAxis ==0); 	
	my @EQPID;
	foreach my $EQPID(@xAxis){
		$EQPID=&EQPIDtrans('2EMS',$EQPID);
		push @EQPID,"\'$EQPID\'";
	}
	my $sql_in=join(',',@EQPID);
	my $dbh=DBI->connect(&getconn('tjn','promis')) or die "{\"result\":\"$!\"}";
	my $sql=qq{Select EQPID,STATUS,LASTSTATUS,(CHANGEDT-to_date('1970-1-1 08:00:00','yyyy-mm-dd hh24:mi:ss'))*24*60*60 from EQPS where EQPID in ($sql_in) and CHANGEDT>=to_date(?,'yyyy-mm-dd hh24:mi:ss') and CHANGEDT<=to_date(?,'yyyy-mm-dd hh24:mi:ss') order by CHANGEDT};
	$sth = $dbh->prepare($sql) or die $!;
	$sth->execute(strftime('%F %T',localtime($s_time)),strftime('%F %T',localtime($e_time))) or die $!;
	$sth->bind_columns(undef,\$equimentid,\$STATUS,\$LASTSTATUS,\$CHANGEDT);
        while ( $sth->fetch() ) {
		my %info;
		$host=&EQPIDtrans('2host',$equimentid);
                if (defined  $unit{$host}){
                        next if ($STATUS eq $LASTSTATUS);
                }
		$info{c_status}=$STATUS;
		$info{l_status}=$LASTSTATUS;
		$info{'time'}=int($CHANGEDT);
		if (defined $unit{$host}){
			my $ref=$unit{$host};	
			push @$ref,\%info;
		}else{
			my @data=(\%info);
			$unit{$host}=\@data;
		}
        }
	$dbh->disconnect(); 
	return \%unit;
}
my $unit=&query_mode;
my @xAxis=sort{ $a cmp $b } keys %$unit;
my @series=&cal_mode(\@xAxis,$unit);
my $EMS_unit=&query_EMS(\@xAxis);
#print Dumper($EMS_unit);
my @EMS_series=&cal_EMS(\@xAxis,$EMS_unit);
push @series,@EMS_series;
#print Dumper(\@EMS_series);
my %output;
$output{xAxis}=\@xAxis;
$output{series}=\@series;
my $json=to_json(\%output);
#print Dumper($unit);
print "$json";
